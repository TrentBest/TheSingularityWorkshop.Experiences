using System.Numerics;

namespace TheSingularityWorkshop.Games.Slots;

/// <summary>An exact, reduced rational number backed by arbitrary-precision integers.</summary>
public readonly struct ExactRational : IEquatable<ExactRational>
{
    public BigInteger Numerator { get; }
    public BigInteger Denominator { get; }

    public ExactRational(BigInteger numerator, BigInteger denominator)
    {
        if (denominator.IsZero)
            throw new DivideByZeroException("A rational denominator cannot be zero.");

        if (denominator.Sign < 0)
        {
            numerator = BigInteger.Negate(numerator);
            denominator = BigInteger.Negate(denominator);
        }

        var divisor = BigInteger.GreatestCommonDivisor(BigInteger.Abs(numerator), denominator);
        Numerator = numerator / divisor;
        Denominator = denominator / divisor;
    }

    public static ExactRational Zero => new(BigInteger.Zero, BigInteger.One);
    public static ExactRational One => new(BigInteger.One, BigInteger.One);

    /// <summary>Converts a decimal to its exact integer-ratio representation.</summary>
    public static ExactRational FromDecimal(decimal value)
    {
        var bits = decimal.GetBits(value);
        var magnitude = (BigInteger)(uint)bits[0]
            | ((BigInteger)(uint)bits[1] << 32)
            | ((BigInteger)(uint)bits[2] << 64);
        var scale = (bits[3] >> 16) & 0x7F;
        if ((bits[3] & int.MinValue) != 0)
            magnitude = BigInteger.Negate(magnitude);

        return new ExactRational(magnitude, BigInteger.Pow(10, scale));
    }

    /// <summary>
    /// Converts to decimal, rounded to at most the requested number of fractional digits.
    /// The rational value itself remains exact; this is only a display/interoperability conversion.
    /// </summary>
    public decimal ToDecimal(int maximumFractionalDigits = 28)
    {
        if (maximumFractionalDigits is < 0 or > 28)
            throw new ArgumentOutOfRangeException(nameof(maximumFractionalDigits));

        var negative = Numerator.Sign < 0;
        var magnitude = BigInteger.Abs(Numerator);
        var decimalMaximum = new BigInteger(decimal.MaxValue);
        var scale = maximumFractionalDigits;
        BigInteger scaled;
        BigInteger factor;

        while (true)
        {
            factor = BigInteger.Pow(10, scale);
            scaled = BigInteger.Divide(
                magnitude * factor + Denominator / 2,
                Denominator);

            if (scaled <= decimalMaximum)
                break;

            if (scale == 0)
                throw new OverflowException("The rational value is outside the decimal range.");

            scale--;
        }

        var whole = BigInteger.DivRem(scaled, factor, out var fraction);
        var result = (decimal)whole;
        if (scale > 0)
            result += (decimal)fraction / (decimal)factor;

        return negative ? -result : result;
    }

    public static ExactRational operator +(ExactRational left, ExactRational right) =>
        new(
            left.Numerator * right.Denominator + right.Numerator * left.Denominator,
            left.Denominator * right.Denominator);

    public static ExactRational operator *(ExactRational left, ExactRational right) =>
        new(left.Numerator * right.Numerator, left.Denominator * right.Denominator);

    public bool Equals(ExactRational other) =>
        Numerator == other.Numerator && Denominator == other.Denominator;

    public override bool Equals(object? obj) => obj is ExactRational other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);
    public override string ToString() => $"{Numerator}/{Denominator}";
    public static bool operator ==(ExactRational left, ExactRational right) => left.Equals(right);
    public static bool operator !=(ExactRational left, ExactRational right) => !left.Equals(right);
}
