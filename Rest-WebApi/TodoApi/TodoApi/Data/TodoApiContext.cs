using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TodoApi.Models
{
    public class TodoApiContext : DbContext
    {
        public TodoApiContext (DbContextOptions<TodoApiContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TodoItem>().HasData(new TodoItem { Id = 1, Name = "Get some bier", IsComplete = true });
            modelBuilder.Entity<TodoItem>().HasData(new TodoItem { Id = 2, Name = "Get some bread", IsComplete = false });

        }

        public DbSet<TodoApi.Models.TodoItem> TodoItem { get; set; }
    }
}
