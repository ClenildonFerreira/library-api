using LibraryApi.Features.Student.models;
using LibraryApi.Infrastructure.Pagination;

namespace LibraryApi.Features.Student.views;

public class StudentListViewModel
{
    public PagedResult<StudentDto>? Students { get; set; }
}