﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CookbookAPI.DTOs;
using CookbookAPI.Requests.Recipes;
using CookbookAPI.ViewModels;
using CookbookAPI.ViewModels.Recipes;

namespace CookbookAPI.Services.Interfaces
{
    public interface IIngredientsService
    {
        public Task<PaginatedList<IngredientDto>> GetAll(GetRecipesRequest request);

        public Task<GetRecipeVm> GetById(int id);

        public Task<int> Create(RecipeRequest request);

        public Task Update(int id, RecipeRequest request);

        public Task Delete(int id);
    }
}
