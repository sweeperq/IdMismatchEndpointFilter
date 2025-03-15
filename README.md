# Id Mismatch Endpoint Filter

## Backstory
A typical API endpoint address for a PUT request is something like `/api/v1/people/123`, where **123** is the `Id` of the person.

We often use FluentValidation to validate our minimal API models via an endpoint filter. In order to perform unique checks
in the validator for update/edit operations, the validator needs access to the `Id` of the model. Below is an example constructor
and unique check for a Person update validator:

```
public PersonUpdateValidator(IDbContextFactory<AppDbContext> dbContextFactory)
{
    RuleFor(x => x.Name)
       .NotEmpty()
       .MaxLength(50)
       .MustAsync(BeUniqueName).WithMessage("'{PropertyName}' must be unique");
}
    
protected async Task<bool> BeUniqueName(MyModel modek, string name, CancellationToken cancellationToken = default) {
    using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
    return !await _context.People.Any(x => x.Name == name && x.Id != id, cancellationToken);
}
```

The problem is that there is nothing preventing the API consumer from using an `Id` of `123` in the URL, and then submitting a 
`Person` model via the request body with an `Id` of `456`. This should result in a `BadRequest` response.

You could enforce this from the minimal API method like so:

```
group.MapPut("{id:int}", async Task<Results<BadRequest<string>, NotFound, NoContent>> (AppDbContext db, int id, PersonUpdateModel model) => {
  if (id != model.Id) 
  {
      return TypedResults.BadRequest("Id mismatch");
  }
  // ... rest of update code
}).Validate<PersonUpdateModel>();
```

The problem with this is that the endpoint validation filter runs before the body of the Put method. Validation with potential database
queries could be happening with the wront `Id` value. This is not ideal.

## The Solution

We created a simple endpoint filter that allows you to validate the `Id` of the model against the `Id` from the route. Using an extension method,
this is what the above minimal API method would look like:
```
group.MapPut("{id:int}", async Task<Results<BadRequest<string>, NotFound, NoContent>> (AppDbContext db, int id, PersonUpdateModel model) => {
  
  // ... rest of update code

}).IdMismatch("id", "Id").Validate<PersonUpdateModel>();
```

If there is an Id mismatch, a `BadRequest<string>` with the default message "Id mismatch" is returned.

## Usage

Usage is straight-forward, and is just two simple steps...

1. Download the NuGet package `Kimmel.IdMismatchEndpointFilter`
1. Add `.IdMismatch()` to the end of your PUT API method