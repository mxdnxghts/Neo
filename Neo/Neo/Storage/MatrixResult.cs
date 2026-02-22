using System;

namespace Neo.Storage;
public sealed class MatrixResult
{
    public Guid Id { get; } = Guid.CreateVersion7();
    public string Result { get; set; }
}
