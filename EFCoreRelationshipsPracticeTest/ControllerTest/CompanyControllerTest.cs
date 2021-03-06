using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using EFCoreRelationshipsPractice;
using EFCoreRelationshipsPractice.Dtos;
using EFCoreRelationshipsPractice.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;
using EFCoreRelationshipsPractice.Services;

namespace EFCoreRelationshipsPracticeTest
{
    public class CompanyControllerTest : TestBase
    {
        public CompanyControllerTest(CustomWebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        public CompanyDbContext GetContext()
        {
            var scope = Factory.Services.CreateScope();
            var scopedServices = scope.ServiceProvider;

            return scopedServices.GetRequiredService<CompanyDbContext>();
        }

        [Fact]
        public async Task Should_create_company_employee_profile_success1()
        {
            var client = GetClient();
            CompanyDto companyDto = new CompanyDto();
            companyDto.Name = "IBM";
            companyDto.Employees = new List<EmployeeDto>()
            {
                new EmployeeDto()
                {
                    Name = "Tom",
                    Age = 19,
                },
            };

            companyDto.Profile = new ProfileDto()
            {
                RegisteredCapital = 100010,
                CertId = "100",
            };

            var httpContent = JsonConvert.SerializeObject(companyDto);
            StringContent content = new StringContent(httpContent, Encoding.UTF8, MediaTypeNames.Application.Json);
            await client.PostAsync("/companies", content);

            var allCompaniesResponse = await client.GetAsync("/companies");
            var body = await allCompaniesResponse.Content.ReadAsStringAsync();

            var returnCompanies = JsonConvert.DeserializeObject<List<CompanyDto>>(body);

            Assert.Equal(1, returnCompanies.Count);
            Assert.Equal(companyDto.Employees.Count, returnCompanies[0].Employees.Count);
            Assert.Equal(companyDto.Employees[0].Age, returnCompanies[0].Employees[0].Age);
            Assert.Equal(companyDto.Employees[0].Name, returnCompanies[0].Employees[0].Name);
            Assert.Equal(companyDto.Profile.CertId, returnCompanies[0].Profile.CertId);
            Assert.Equal(companyDto.Profile.RegisteredCapital, returnCompanies[0].Profile.RegisteredCapital);

            var scope = Factory.Services.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var context = scopedServices.GetRequiredService<CompanyDbContext>();
            Assert.Equal(1, context.Companies.ToList().Count);
            var firstCompany = await context.Companies.Include(company => company.Profile).FirstOrDefaultAsync();
            Assert.Equal(companyDto.Profile.CertId, firstCompany.Profile.CertId);
        }

        [Fact]
        public async Task Should_delete_company_and_related_employee_and_profile_success()
        {
            var client = GetClient();
            CompanyDto companyDto = new CompanyDto();
            companyDto.Name = "IBM";
            companyDto.Employees = new List<EmployeeDto>()
            {
                new EmployeeDto()
                {
                    Name = "Tom",
                    Age = 19,
                },
            };

            companyDto.Profile = new ProfileDto()
            {
                RegisteredCapital = 100010,
                CertId = "100",
            };

            var httpContent = JsonConvert.SerializeObject(companyDto);
            StringContent content = new StringContent(httpContent, Encoding.UTF8, MediaTypeNames.Application.Json);

            var response = await client.PostAsync("/companies", content);
            await client.DeleteAsync(response.Headers.Location);
            var allCompaniesResponse = await client.GetAsync("/companies");
            var body = await allCompaniesResponse.Content.ReadAsStringAsync();

            var returnCompanies = JsonConvert.DeserializeObject<List<CompanyDto>>(body);

            Assert.Equal(0, returnCompanies.Count);
        }

        [Fact]
        public async Task Should_create_many_companies_success()
        {
            var client = GetClient();
            CompanyDto companyDto = new CompanyDto();
            companyDto.Name = "IBM";
            companyDto.Employees = new List<EmployeeDto>()
            {
                new EmployeeDto()
                {
                    Name = "Tom",
                    Age = 19,
                },
            };

            companyDto.Profile = new ProfileDto()
            {
                RegisteredCapital = 100010,
                CertId = "100",
            };

            var httpContent = JsonConvert.SerializeObject(companyDto);
            StringContent content = new StringContent(httpContent, Encoding.UTF8, MediaTypeNames.Application.Json);
            await client.PostAsync("/companies", content);
            await client.PostAsync("/companies", content);

            var allCompaniesResponse = await client.GetAsync("/companies");
            var body = await allCompaniesResponse.Content.ReadAsStringAsync();

            var returnCompanies = JsonConvert.DeserializeObject<List<CompanyDto>>(body);

            Assert.Equal(2, returnCompanies.Count);
        }

        [Fact]
        public async Task Should_create_company_employee_profile_success_Service_Test()
        {
            //given
            CompanyDto companyDto = new CompanyDto();
            companyDto.Name = "IBM";
            companyDto.Employees = new List<EmployeeDto>()
            {
                new EmployeeDto()
                {
                    Name = "Tom",
                    Age = 19,
                },
            };

            companyDto.Profile = new ProfileDto()
            {
                RegisteredCapital = 100010,
                CertId = "100",
            };

            var context = GetContext();
            var companyService = new CompanyService(context);

            //when
            await companyService.AddCompany(companyDto);

            //then
            Assert.Equal(1, context.Companies.Count());

            var actualCompany = await context.Companies
                .Include(company => company.Profile)
                .Include(company => company.Employees)
                .FirstOrDefaultAsync();
            Assert.Equal(companyDto, new CompanyDto(actualCompany));
        }

        [Fact]
        public async Task Should_create_many_companies_success_Service_Test()
        {
            //given
            CompanyDto companyDto = new CompanyDto();
            companyDto.Name = "IBM";
            companyDto.Employees = new List<EmployeeDto>()
            {
                new EmployeeDto()
                {
                    Name = "Tom",
                    Age = 19,
                },
            };

            companyDto.Profile = new ProfileDto()
            {
                RegisteredCapital = 100010,
                CertId = "100",
            };

            var context = GetContext();
            var companyService = new CompanyService(context);

            //when
            await companyService.AddCompany(companyDto);
            await companyService.AddCompany(companyDto);

            //then
            Assert.Equal(2, context.Companies.Count());
        }

        [Fact]
        public async Task Should_get_all_return_all_companies_Service_Test()
        {
            //given
            CompanyDto companyDto = new CompanyDto();
            companyDto.Name = "IBM";
            companyDto.Employees = new List<EmployeeDto>()
            {
                new EmployeeDto()
                {
                    Name = "Tom",
                    Age = 19,
                },
            };

            companyDto.Profile = new ProfileDto()
            {
                RegisteredCapital = 100010,
                CertId = "100",
            };

            var context = GetContext();
            var companyService = new CompanyService(context);
            await companyService.AddCompany(companyDto);
            await companyService.AddCompany(companyDto);

            //when
            var companies = await companyService.GetAll();

            //then
            Assert.Equal(2, context.Companies.Count());
            Assert.Equal(new List<CompanyDto>() { companyDto, companyDto }, companies);
        }

        [Fact]
        public async Task Should_delete_company_and_related_employee_and_profile_success_Service_Test()
        {
            //given
            CompanyDto companyDto = new CompanyDto();
            companyDto.Name = "IBM";
            companyDto.Employees = new List<EmployeeDto>()
            {
                new EmployeeDto()
                {
                    Name = "Tom",
                    Age = 19,
                },
            };

            companyDto.Profile = new ProfileDto()
            {
                RegisteredCapital = 100010,
                CertId = "100",
            };

            var context = GetContext();
            var companyService = new CompanyService(context);
            var id = await companyService.AddCompany(companyDto);

            //when
            await companyService.DeleteCompany(id);

            //then
            Assert.Equal(0, context.Companies.Count());
            Assert.Equal(0, context.Companies.Count());
            Assert.Equal(0, context.Companies.Count());
        }

        [Fact]
        public async Task Should_get_correct_company_by_id_Service_Test()
        {
            //given
            CompanyDto companyDto = new CompanyDto();
            companyDto.Name = "IBM";
            companyDto.Employees = new List<EmployeeDto>()
            {
                new EmployeeDto()
                {
                    Name = "Tom",
                    Age = 19,
                },
            };

            companyDto.Profile = new ProfileDto()
            {
                RegisteredCapital = 100010,
                CertId = "100",
            };

            CompanyDto companyDto2 = new CompanyDto();
            companyDto2.Profile = new ProfileDto()
            {
                RegisteredCapital = 20002,
                CertId = "110",
            };
            companyDto2.Name = "I2BM";
            companyDto2.Employees = new List<EmployeeDto>()
            {
                new EmployeeDto()
                {
                    Name = "Tomy",
                    Age = 20,
                },
            };

            var context = GetContext();
            var companyService = new CompanyService(context);
            await companyService.AddCompany(companyDto2);
            var id = await companyService.AddCompany(companyDto);

            //when
            var acutalCompany = await companyService.GetById(id);

            //then
            Assert.Equal(companyDto, acutalCompany);
        }
    }
}