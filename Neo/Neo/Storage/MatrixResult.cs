using System;

namespace Neo.Storage;
public sealed class MatrixResult
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Result { get; set; }
}
