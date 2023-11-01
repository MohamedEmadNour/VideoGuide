using Microsoft.EntityFrameworkCore;
using VideoGuide.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

public class DataNotExistsAttribute : ValidationAttribute
{
    private readonly Type _entityType;
    private readonly string _propertyName;

    public DataNotExistsAttribute(Type entityType, string propertyName)
    {
        _entityType = entityType;
        _propertyName = propertyName;
    }

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    {
        if (value == null)
        {
            return new ValidationResult(ErrorMessage);
        }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        var dbContext = (VideoGuideContext)validationContext.GetService(typeof(VideoGuideContext));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
        var dbSet = GetDbSet(dbContext, _entityType);
#pragma warning restore CS8604 // Possible null reference argument.
        var predicate = GetPredicate(_propertyName, value);

        var anyExists = Any(dbSet, predicate);
        if (anyExists)
        {
            return new ValidationResult(ErrorMessage);
        }

#pragma warning disable CS8603 // Possible null reference return.
        return ValidationResult.Success;
#pragma warning restore CS8603 // Possible null reference return.
    }

    private static object GetDbSet(VideoGuideContext dbContext, Type entityType)
    {
        // Find the DbSet property for the specified entityType.
        var dbSetProperty = dbContext.GetType().GetProperties()
            .SingleOrDefault(p =>
                p.PropertyType.IsGenericType &&
                p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                p.PropertyType.GetGenericArguments().SingleOrDefault() == entityType);

        if (dbSetProperty == null)
        {
            throw new InvalidOperationException($"DbSet for entity type '{entityType.Name}' not found.");
        }

        // Get the value of the DbSet property.
#pragma warning disable CS8603 // Possible null reference return.
        return dbSetProperty.GetValue(dbContext);
#pragma warning restore CS8603 // Possible null reference return.
    }


    private Expression GetPredicate(string propertyName, object value)
    {
        var parameter = Expression.Parameter(_entityType, "entity");
        var property = _entityType.GetProperty(propertyName);

        if (property == null)
        {
            throw new ArgumentException($"Property '{propertyName}' not found in entity '{_entityType.Name}'.");
        }

        var propertyAccess = Expression.Property(parameter, property);
        var constantValue = Expression.Constant(value, property.PropertyType);
        var equalExpression = Expression.Equal(propertyAccess, constantValue);
        return Expression.Lambda(equalExpression, parameter);
    }

    private bool Any(object dbSet, Expression predicate)
    {
        var anyMethod = typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .FirstOrDefault(method => method.Name == "Any" && method.GetParameters().Length == 2);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var genericAnyMethod = anyMethod.MakeGenericMethod(_entityType);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8605 // Unboxing a possibly null value.
        return (bool)genericAnyMethod.Invoke(null, new[] { dbSet, predicate });
#pragma warning restore CS8605 // Unboxing a possibly null value.
    }
}
