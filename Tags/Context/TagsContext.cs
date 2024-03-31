﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Tags.Models;

namespace Tags.Context;

public class TagsContext(DbContextOptions options) : DbContext(options)
{
    public const string CalcuateCountPercentFunctionName = "CalculateCountPercent";
    public const string TagCountColumnName = "Count";
    public const string TagTableName = "Tags";

    public DbSet<TagModel> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        const string CountPercentComputedSql = $"dbo.{CalcuateCountPercentFunctionName}(CAST(\"{TagCountColumnName}\" AS DECIMAL(38,18)))";

        modelBuilder
            .HasDbFunction(typeof(TagsContext)
                .GetMethod(nameof(CalculateCountPercent))!)
            .HasName(CalcuateCountPercentFunctionName);

        modelBuilder
            .Entity<TagModel>()
            .ToTable(TagTableName)
            .Property(e => e.CountPercent)
            .HasComputedColumnSql(CountPercentComputedSql)
            .ValueGeneratedNever()
            .Metadata
                .SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);

        base.OnModelCreating(modelBuilder);
    }

    public float CalculateCountPercent(decimal count) => throw new NotSupportedException();
}
