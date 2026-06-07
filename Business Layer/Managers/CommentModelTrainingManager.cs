using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Business_Layer.MLModels.CommentToxicityModels;
using Core_Layer.IServices;
using Microsoft.ML;

namespace Business_Layer.Managers
{
    public class CommentModelTrainingManager : ICommentModelTrainingService
    {
        public Task<string> TTrainAndSaveModelAsync()
        {
            try
            {
                string dataPath = Path.Combine(Environment.CurrentDirectory, "ecommerce_reviews.csv");
                string modelPath = Path.Combine(Environment.CurrentDirectory, "MLModels", "ToxicityModel.zip");

                if (!File.Exists(dataPath))
                    return Task.FromResult("Error: CSV data file not found!");

                var lines = File.ReadAllLines(dataPath);
                var dataList = new List<ToxicityModelInput>();

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("Text")) continue;

                    int lastCommaIndex = line.LastIndexOf(',');
                    if (lastCommaIndex == -1) continue;

                    string textPart = line.Substring(0, lastCommaIndex).Trim('"', ' ');
                    string labelPart = line.Substring(lastCommaIndex + 1).Trim();

                    bool isToxic = (labelPart == "1" || labelPart.ToLower() == "true");

                    dataList.Add(new ToxicityModelInput
                    {
                        Text = textPart,
                        Label = isToxic
                    });
                }

                int toxicCount = dataList.Count(x => x.Label);
                int cleanCount = dataList.Count(x => !x.Label);

                if (dataList.Count == 0)
                    return Task.FromResult("Error: No data could be read! The file may be empty.");

                MLContext mlContext = new MLContext(seed: 0);
                IDataView dataView = mlContext.Data.LoadFromEnumerable(dataList);

                // DEĞİŞİKLİK 1: Veriyi Eğitim (%80) ve Test (%20) olarak ikiye bölüyoruz
                var trainTestData = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2, seed: 0);

                var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(ToxicityModelInput.Text))
                    .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                        labelColumnName: nameof(ToxicityModelInput.Label),
                        featureColumnName: "Features"));

                // DEĞİŞİKLİK 2: Modeli SADECE Eğitim (TrainSet) verisiyle eğitiyoruz
                ITransformer trainedModel = pipeline.Fit(trainTestData.TrainSet);

                // DEĞİŞİKLİK 3: Modeli Test (TestSet) verisiyle değerlendiriyoruz
                IDataView predictions = trainedModel.Transform(trainTestData.TestSet);
                var metrics = mlContext.BinaryClassification.Evaluate(
                    data: predictions,
                    labelColumnName: nameof(ToxicityModelInput.Label));

                var modelDir = Path.GetDirectoryName(modelPath);
                if (!Directory.Exists(modelDir))
                    Directory.CreateDirectory(modelDir!);

                // Modeli kaydederken TrainSet şemasını baz alıyoruz
                mlContext.Model.Save(trainedModel, trainTestData.TrainSet.Schema, modelPath);

                // DEĞİŞİKLİK 4: Çıktı mesajına Accuracy ve F1 Score değerlerini ekliyoruz.
                string successMessage = $"\r\nSuccess: AI Model trained! Total: {cleanCount + toxicCount} data. " +
                                        $"| Accuracy: {metrics.Accuracy:P2} " +
                                        $"| F1 Score: {metrics.F1Score:P2} " +
                                        $"| AUC: {metrics.AreaUnderRocCurve:P2}";

                return Task.FromResult(successMessage);
            }
            catch (Exception ex)
            {
                return Task.FromResult($"Error: Training failed. Details: {ex.Message}");
            }
        }
    }
}