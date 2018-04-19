using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning.Preview;

// INPUT:  VideoFrame
// OUTPUT: List<string> appel�e �classLabel� et Dictionary<string,float> appel� �loss�

namespace WinMLHelloWorld.Models
{
    //
    // TODO: pour que le code soit plus lisible
    //       remplacer le nom des classes g�n�r�es par le nom de notre mod�le
    public sealed class FoodModelInput
    {
        public VideoFrame data { get; set; }
    }

    public sealed class FoodModelOutput
    {
        public IList<string> classLabel { get; set; }
        public IDictionary<string, float> loss { get; set; }

        public FoodModelOutput()
        {
            this.classLabel = new List<string>();
            this.loss = new Dictionary<string, float>();

            // On ajoute nos cat�gories
            // sans cel�, nous pourrions avoir une exception de type 
            // "The binding is incomplete or does not match the input/output description. (Exception from HRESULT: 0x88900002)"
            this.loss.Add("burger", float.NaN);
            this.loss.Add("hot dog", float.NaN);
            this.loss.Add("taco", float.NaN);
        }
    }

    public sealed class FoodModel
    {
        private LearningModelPreview learningModel;
        public static async Task<FoodModel> CreateFoodModel(StorageFile file)
        {
            LearningModelPreview learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            FoodModel model = new FoodModel();
            model.learningModel = learningModel;
            return model;
        }

        public async Task<FoodModelOutput> EvaluateAsync(FoodModelInput input) {
            FoodModelOutput output = new FoodModelOutput();
            LearningModelBindingPreview binding = new LearningModelBindingPreview(learningModel);
            binding.Bind("data", input.data);
            binding.Bind("classLabel", output.classLabel);
            binding.Bind("loss", output.loss);
            LearningModelEvaluationResultPreview evalResult = await learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}
