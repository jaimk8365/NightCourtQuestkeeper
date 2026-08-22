using NightCourt.Core;
using UnityEngine;
namespace NightCourt.Runtime
{
    public sealed class CraftingStation3D:MonoBehaviour
    {
        private readonly Recipe recipe=new Recipe{Id="glow_shelf",OutputItemId="furniture.glow_shelf",RequiredSteps=2,Ingredients={new Ingredient{ItemId="material.moonwood",Quantity=3}}};
        private void OnMouseUpAsButton()
        {
            GameBootstrap game=GameBootstrap.Instance;if(game==null)return;var service=new PersistentCraftingService(game.Save);
            BuildProgressState build=game.Save.Builds.Find(x=>x.RecipeId==recipe.Id);
            if(build==null)
            {
                if(service.Quantity(recipe.OutputItemId)>0){CozyHud.Instance?.Notify("Your Glowcap Shelf is already lighting the cottage");return;}
                if(!service.Start(recipe)){CozyHud.Instance?.Notify("Need 3 Moonwood · complete quests to gather more");return;}
                game.Persist();CozyHud.Instance?.Notify("Glowcap Shelf started · one more visit will shape it");return;
            }
            int progress=service.AddStep(recipe);game.Persist();
            if(service.Quantity(recipe.OutputItemId)>0){HousingDisplay.Instance?.Refresh();CozyHud.Instance?.Notify("Glowcap Shelf crafted! Your cottage changed ✦");}
            else CozyHud.Instance?.Notify($"Build step {progress}/{recipe.RequiredSteps} complete");
        }
    }
}
