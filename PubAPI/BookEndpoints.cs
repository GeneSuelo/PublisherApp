using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using PublisherData;
using PublisherDomain;

namespace PubAPI;

public static class BookEndpoints
{
    public static void MapBookEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Book").WithTags(nameof(Book));

        // READ ALL
        group.MapGet("/", async (PubContext db) =>
            await db.Books
                    .Include(b => b.Author)
                    .Include(b => b.Cover)
                    .ToListAsync()
        )
        .WithName("GetAllBooks")
        .WithOpenApi();

        // READ ONE
        group.MapGet("/{id}", async Task<Results<Ok<Book>, NotFound>> (int id, PubContext db) =>
        {
            var book = await db.Books
                               .Include(b => b.Author)
                               .Include(b => b.Cover)
                                
                               .FirstOrDefaultAsync(b => b.BookId == id);
            return book is not null
                ? TypedResults.Ok(book)
                : TypedResults.NotFound();
        })
        .WithName("GetBookById")
        .WithOpenApi();

        // CREATE
        group.MapPost("/", async Task<Created<Book>> (Book book, HttpContext http, PubContext db) =>
        {
            db.Books.Add(book);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/Book/{book.BookId}", book);
        })
        .WithName("CreateBook")
        .WithOpenApi();

        // UPDATE
        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, Book input, PubContext db) =>
        {
            var book = await db.Books.FindAsync(id);
            if (book is null) return TypedResults.NotFound();

            book.Title = input.Title;
            book.PublishDate = input.PublishDate;
            book.AuthorId = input.AuthorId;
            book.BasePrice = input.BasePrice;
            // …any other props…

            await db.SaveChangesAsync();
            return TypedResults.Ok();
        })
        .WithName("UpdateBook")
        .WithOpenApi();

        // DELETE
        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, PubContext db) =>
        {
            var book = await db.Books.FindAsync(id);
            if (book is null) return TypedResults.NotFound();

            db.Books.Remove(book);
            await db.SaveChangesAsync();
            return TypedResults.Ok();
        })
        .WithName("DeleteBook")
        .WithOpenApi();
    }
}
