using System;
using System.Collections.Generic;
using System.Linq;

namespace NightCourt.Core
{
    public sealed class Ingredient { public string ItemId { get; set; } = ""; public int Quantity { get; set; } }
    public sealed class Recipe
    {
        public string Id { get; set; } = "";
        public string OutputItemId { get; set; } = "";
        public int RequiredSteps { get; set; } = 1;
        public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    }
    public sealed class CraftingSystem
    {
        private readonly Dictionary<string, int> inventory = new Dictionary<string, int>();
        private readonly Dictionary<string, int> builds = new Dictionary<string, int>();
        public int Quantity(string itemId) => inventory.TryGetValue(itemId, out int value) ? value : 0;
        public void Add(string itemId, int quantity) { if (quantity > 0) inventory[itemId] = Quantity(itemId) + quantity; }
        public bool Start(Recipe recipe)
        {
            if (!recipe.Ingredients.All(i => Quantity(i.ItemId) >= i.Quantity)) return false;
            foreach (Ingredient i in recipe.Ingredients) inventory[i.ItemId] -= i.Quantity;
            builds[recipe.Id] = 0; return true;
        }
        public bool AddStep(Recipe recipe)
        {
            if (!builds.TryGetValue(recipe.Id, out int progress)) return false;
            progress++; builds[recipe.Id] = progress;
            if (progress >= Math.Max(1, recipe.RequiredSteps)) { Add(recipe.OutputItemId, 1); builds.Remove(recipe.Id); }
            return true;
        }
    }
}
