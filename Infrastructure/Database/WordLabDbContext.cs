using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public partial class WordLabDbContext : DbContext
{
    public WordLabDbContext()
    {
    }

    public WordLabDbContext(DbContextOptions<WordLabDbContext> options)
        : base(options)
    {
    }

    public DbSet<Demotable> Demotables { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

    }
}
