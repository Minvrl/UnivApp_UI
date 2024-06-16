using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Univ.Service.Dtos;

namespace Univ.Service.Services.Interfaces
{
    public interface IStudentService
    {
        int Create(StudentCreateDto createDto);
        void Update(int id, StudentUpdateDto updateDto);
        StudentGetIdDto GetById(int id);
        List<StudentGetDto> GetAll(string? search = null);
		PaginatedList<StudentGetDto> GetAllPaginated(int page = 1, int size = 10);
		void Delete(int id);
    }
}
