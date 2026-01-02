using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FluentResults;
using Neo.Services;
using Npgsql;

namespace Neo.Storage;
public sealed class StorageHandler(string connectionString)
{
    public async Task<Result> SaveAsync(Solver solver)
    {
        using var connection = new NpgsqlConnection(connectionString);

        var sql = """
            insert into matrixresults (id, result)
            values (@id, @result)
            """;
        var matrixResult = new MatrixResult()
        {
            Result = solver.ToString()
        };
        var exec = await connection.ExecuteAsync(sql, new
        {
            id = matrixResult.Id,
            result = matrixResult.Result 
        });

        return exec == 1 ? Result.Ok() : Result.Fail("Failed to save");
    }
    public Result Save(Solver solver)
    {
        using var connection = new NpgsqlConnection(connectionString);

        var sql = """
            insert into matrixresults (id, result)
            values (@id, @result)
            """;
        var matrixResult = new MatrixResult()
        {
            Result = solver.ToString()
        };
        var exec = connection.Execute(sql, new
        {
            id = matrixResult.Id,
            result = matrixResult.Result 
        });

        return exec == 1 ? Result.Ok() : Result.Fail("Failed to save");
    }

    public async Task<Result> UpdateAsync(MatrixResult matrixResult)
    {
        using var connection = new NpgsqlConnection(connectionString);

        var sql = """
            update matrixresults
            set result = @result
            where id = @id
            """;
        var exec = await connection.ExecuteAsync(sql, new
        {
            id = matrixResult.Id,
            result = matrixResult.Result
        });

        return exec == 1 ? Result.Ok() : Result.Fail("Failed to save");
    }

    public async Task<List<MatrixResult>> ReceiveResults()
    {
        using var connection = new NpgsqlConnection(connectionString);

        var sql = """
            select id, result
            from matrixresults
            """;
        var matrixResults = (await connection.QueryAsync<MatrixResult>(sql)).ToList();

        return matrixResults;
    }

    public async Task<Result> DeleteResult(Guid id)
    {
        using var connection = new NpgsqlConnection(connectionString);

        var sql = """
            delete from matrixresults
            where id = @id
            """;
        var exec = await connection.ExecuteAsync(sql, new
        {
            id,
        });

        return exec == 1 ? Result.Ok() : Result.Fail("Failed to save");
    }
}
