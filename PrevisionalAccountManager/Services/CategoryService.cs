using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PrevisionalAccountManager.Models;
using PrevisionalAccountManager.Models.DataBaseEntities;

namespace PrevisionalAccountManager.Services
{
    public interface ICategoryService
    {
        IReadOnlyList<CategoryModel> GetAllCategories();
        Task<IReadOnlyList<CategoryModel>> GetAllCategoriesAsync();
        CategoryModel? GetCategoryById(int id);
        CategoryModel AddCategory(string name);
        CategoryModel? GetCategoryByName(string name);
        Task<CategoryModel> AddCategoryAsync(string name);
        void RemoveCategory(CategoryModel category);
    }

    public class CategoryService(DatabaseContext databaseContext, ILoginService loginService) : IDisposable, ICategoryService
    {
        public IReadOnlyList<CategoryModel> GetAllCategories()
        {
            return GetAllCategoryQuery().ToList();
        }

        public async Task<IReadOnlyList<CategoryModel>> GetAllCategoriesAsync()
        {
            return await GetAllCategoryQuery().ToListAsync();
        }

        private IOrderedQueryable<CategoryModel> GetAllCategoryQuery()
        {
            return databaseContext.Categories.Where(c => c.OwnerUserId == loginService.CurrentUserId).OrderBy(c => c.Name);
        }

        public CategoryModel? GetCategoryById(int id)
        {
            return databaseContext.Categories.Find(id);
        }

        public CategoryModel AddCategory(string name)
        {
            if ( string.IsNullOrWhiteSpace(name) )
                throw new ArgumentException("Category name cannot be empty");

            // Check if category already exists
            var existingCategory = GetCategoryByName(name);
            if ( existingCategory != null )
                return existingCategory;

            var category = new CategoryModel { Name = name.Trim() };
            SetCategoryOwnerUserId(category);
            databaseContext.Categories.Add(category);
            databaseContext.SaveChanges();
            return category;
        }

        private void SetCategoryOwnerUserId(CategoryModel category)
        {
            category.OwnerUserId = loginService.CurrentUserId!.Value;
        }

        public CategoryModel? GetCategoryByName(string name)
        {
            return databaseContext.Categories.FirstOrDefault(FindCategoryByName(name));
        }

        public async Task<CategoryModel> AddCategoryAsync(string name)
        {
            if ( string.IsNullOrWhiteSpace(name) )
                throw new ArgumentException("Category name cannot be empty");

            var existingCategory = await databaseContext.Categories
                .FirstOrDefaultAsync(FindCategoryByName(name));
            if ( existingCategory != null )
                return existingCategory;

            var category = new CategoryModel { Name = name.Trim() };
            SetCategoryOwnerUserId(category);
            databaseContext.Categories.Add(category);
            await databaseContext.SaveChangesAsync();
            return category;
        }

        private static Expression<Func<CategoryModel, bool>> FindCategoryByName(string name)
        {
            return c => Equals(c.Name, name);
        }

        public void RemoveCategory(CategoryModel category)
        {
            var entity = databaseContext.Categories.Find(category.Id);
            if ( entity != null )
            {
                databaseContext.Categories.Remove(entity);
                databaseContext.SaveChanges();
            }
        }

        public void Dispose()
        {
            databaseContext?.Dispose();
        }
    }
}