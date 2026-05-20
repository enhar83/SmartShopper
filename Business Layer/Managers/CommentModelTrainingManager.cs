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
                    return Task.FromResult("Error: Hiçbir veri okunamadı! Dosya boş olabilir.");

                MLContext mlContext = new MLContext(seed: 0);
                IDataView dataView = mlContext.Data.LoadFromEnumerable(dataList);

                var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(ToxicityModelInput.Text))
                    .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                        labelColumnName: nameof(ToxicityModelInput.Label),
                        featureColumnName: "Features"));

                ITransformer trainedModel = pipeline.Fit(dataView);

                var modelDir = Path.GetDirectoryName(modelPath);
                if (!Directory.Exists(modelDir))
                    Directory.CreateDirectory(modelDir!);

                mlContext.Model.Save(trainedModel, dataView.Schema, modelPath);

                return Task.FromResult($"Success: AI Model başarıyla eğitildi! ({cleanCount} Temiz, {toxicCount} Toksik yorum ezberlendi.)");
            }
            catch (Exception ex)
            {
                return Task.FromResult($"Error: Training failed. Details: {ex.Message}");
            }
        }
    }
}