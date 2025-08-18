using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Extensions
{
    public static class ODataConfigurationExtensions
    {
        public static IServiceCollection AddODataServices(this IServiceCollection services)
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Author>("Authors");
            modelBuilder.EntitySet<Publisher>("Publishers");
            modelBuilder.EntitySet<Category>("Categories");
            modelBuilder.EntitySet<Book>("Books");
            modelBuilder.EntitySet<BookItem>("BookItems");
            modelBuilder.EntitySet<Loan>("Loans");
            modelBuilder.EntitySet<Fine>("Fines");
            modelBuilder.EntitySet<Rating>("Ratings");
            modelBuilder.EntitySet<Comment>("Comments");
            modelBuilder.EntitySet<BookAuthor>("BookAuthors");
            modelBuilder.EntitySet<BookCategory>("BookCategories");

            services.AddControllers()
                .AddOData(options =>
                    options.AddRouteComponents("odata", modelBuilder.GetEdmModel())
                        .Filter()
                        .Select()
                        .Expand()
                        .OrderBy()
                        .Count()
                        .SetMaxTop(null));

            return services;
        }
    }
}