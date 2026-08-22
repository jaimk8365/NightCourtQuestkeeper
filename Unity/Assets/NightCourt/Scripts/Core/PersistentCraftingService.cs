using System;
using System.Linq;
namespace NightCourt.Core
{
    public sealed class PersistentCraftingService
    {
        private readonly PlayerSave save;
        public PersistentCraftingService(PlayerSave save)=>this.save=save??throw new ArgumentNullException(nameof(save));
        public int Quantity(string itemId)=>save.Inventory.FirstOrDefault(x=>x.ItemId==itemId)?.Quantity??0;
        public bool Start(Recipe recipe)
        {
            if(save.Builds.Any(x=>x.RecipeId==recipe.Id)||!recipe.Ingredients.All(i=>Quantity(i.ItemId)>=i.Quantity))return false;
            foreach(Ingredient ingredient in recipe.Ingredients)Change(ingredient.ItemId,-ingredient.Quantity);
            save.Builds.Add(new BuildProgressState{RecipeId=recipe.Id,StepsCompleted=0});return true;
        }
        public int AddStep(Recipe recipe)
        {
            BuildProgressState build=save.Builds.FirstOrDefault(x=>x.RecipeId==recipe.Id);if(build==null)return 0;
            build.StepsCompleted++;
            if(build.StepsCompleted>=Math.Max(1,recipe.RequiredSteps)){Change(recipe.OutputItemId,1);save.Builds.Remove(build);return recipe.RequiredSteps;}
            return build.StepsCompleted;
        }
        private void Change(string itemId,int delta)
        {
            InventoryEntry entry=save.Inventory.FirstOrDefault(x=>x.ItemId==itemId);
            if(entry==null){entry=new InventoryEntry{ItemId=itemId};save.Inventory.Add(entry);}entry.Quantity=Math.Max(0,entry.Quantity+delta);
        }
    }
}
