using System.Data.Entity;
using System.Linq.Dynamic;
using GoatEdu.Core.Interfaces.TagInterfaces;
using GoatEdu.Core.Models;
using GoatEdu.Core.QueriesFilter;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TagRepository : BaseRepository<Tag>, ITagRepository
{
    private readonly GoatEduContext _context;
 
    public TagRepository(GoatEduContext context) : base(context)
    {
        _context = context;
    }

    //
    //
    //
    // Lỗi ở đây nè anh :))
    //
    //
    //
    public async Task<List<Tag>> GetTagByFilters(TagQueryFilter queryFilter)
    {
        var tags = _entities.AsQueryable();
        tags = ApplyFilterSortAndSearch(tags, queryFilter);
        tags = ApplySorting(tags, queryFilter);
        return await tags.ToListAsync();
    }

    public async Task<List<string?>> GetTagNameByNameAsync(List<string?> tagName)
    {
        var result = await _entities.ToListAsync();
        return result.Where(x => tagName.Any(name => name.Equals(x.TagName?.ToUpper()))).Select(x => x.TagName).ToList();
    }
    
    public async Task SoftDelete(List<Guid> guids)
    {
        await _entities.Where(x => guids.Any(id => id == x.Id)).ForEachAsync(a => a.IsDeleted = true);
    }
    
    private IQueryable<Tag> ApplyFilterSortAndSearch(IQueryable<Tag> tags, TagQueryFilter queryFilter)
    {
        tags = tags.Where(x => x.IsDeleted == false);
        
        if (!string.IsNullOrEmpty(queryFilter.Search))
        {
            tags = tags.Where(x => x.TagName.Contains(queryFilter.Search));
        }
        return tags;
    }
    
    private IQueryable<Tag> ApplySorting(IQueryable<Tag> tags, TagQueryFilter queryFilter)
    {
        tags = queryFilter.Sort.ToLower() switch
        {
            "date" => queryFilter.SortDirection.ToLower() == "desc"
                ? tags.OrderByDescending(x => x.CreatedAt)
                : tags.OrderBy(x => x.CreatedAt).ThenBy(x => x.TagName),
            _ => queryFilter.SortDirection.ToLower() == "desc"
                ? tags.OrderByDescending(x => x.TagName)
                : tags.OrderBy(x => x.TagName)
        };
        return tags;
    }
}