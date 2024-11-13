using JA.Expressions;

namespace JA.LinearAlgebra.Mechanics
{
    public record Vec6Expr(Expr X, Expr Y, Expr Z, Expr U, Expr V, Expr W) : ArrayExpr([X, Y, Z, U, V, W])
    {
        public Vec6Expr(Vec3Expr a, Vec3Expr b) : this(a.X, a.Y, a.Z, b.X, b.Y, b.Z) { }
    }
}
