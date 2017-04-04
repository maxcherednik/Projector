using System;

namespace Projector.Data
{
    public interface IField
    {
        Type DataType { get; }

        string Name { get; }
    }

    public interface IField<TData> : IField
    {
        TData Value { get; }
    }
}
