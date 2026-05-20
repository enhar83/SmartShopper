using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

                MLContext mlContext = new MLContext(seed: 0);

                IDataView dataView = mlContext.Data.LoadFromTextFile<ToxicityModelInput>(
                    path: dataPath,
                    hasHeader: true,
                    separatorChar: ',',
                    allowQuoting: true);

                var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(ToxicityModelInput.Text))
                    .Append(mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression(
                        labelColumnName: nameof(ToxicityModelInput.Label),
                        featureColumnName: "Features"
                    ));

                ITransformer trainedModel = pipeline.Fit(dataView);

                var modelDir = Path.GetDirectoryName(modelPath);
                if (!Directory.Exists(modelDir))
                    Directory.CreateDirectory(modelDir!);

                mlContext.Model.Save(trainedModel, dataView.Schema, modelPath);

                return Task.FromResult("Success: AI Model successfully trained and saved.");
            }
            catch (Exception ex)
            {
                return Task.FromResult($"Error: Training failed. Details: {ex.Message}");
            }
        }
    }
}
