using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business_Layer.MLModels.CommentToxicityModels;
using Core_Layer.IServices;
using Microsoft.Extensions.ML;

namespace Business_Layer.Managers
{
    public class ToxicityPredictionManager:IToxicityPredictionService
    {
        private readonly PredictionEnginePool<ToxicityModelInput, ToxicityModelOutput> _predictionEnginePool;

        public ToxicityPredictionManager(PredictionEnginePool<ToxicityModelInput, ToxicityModelOutput> predictionEnginePool)
        {
            _predictionEnginePool = predictionEnginePool;
        }

        public Task<(double ToxicityScore, bool IsToxic)> TPredictToxicityAsync(string text)
        {
            var input = new ToxicityModelInput { Text = text };
            var output = _predictionEnginePool.Predict("ToxicityModel", input);

            return Task.FromResult(((double)output.Probability, output.Prediction));
        }
    }
}
