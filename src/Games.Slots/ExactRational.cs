using System.Numerics;

namespace TheSingularityWorkshop.Games.Slots;

/// <summary>An exact, reduced rational number backed by arbitrary-precision integers.</summary>
/// <summary>An immutable reduced fraction whose arithmetic is exact and uses arbitrary precision.</summary>
public readonly struct ExactRational : IEquatable<ExactRational>
{
    private readonly BigInteger _numerator;
    private readonly BigInteger _denominator;

    /// <summary>Gets the signed numerator in the reduced fraction.</summary>
    public BigInteger Numerator => _numerator;
    /// <summary>Gets the positive denominator in the reduced fraction.</summary>
    public BigInteger Denominator => _denominator.IsZero ? BigInteger.One : _denominator;

    /// <summary>Creates a reduced rational number.</summary>
    /// <param name="numerator">The signed numerator.</param>
    /// <param name="denominator">The non-zero denominator.</param>
    /// <exception cref="DivideByZeroException">The denominator is zero.</exception>
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
        _numerator = numerator / divisor;
        _denominator = denominator / divisor;
    }

    /// <summary>Gets the exact rational value zero.</summary>
    public static ExactRational Zero => new(BigInteger.Zero, BigInteger.One);
    /// <summary>Gets the exact rational value one.</summary>
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

    /// <summary>Adds two exact rational values.</summary>
    public static ExactRational operator +(ExactRational left, ExactRational right) =>
        new(
            left.Numerator * right.Denominator + right.Numerator * left.Denominator,
            left.Denominator * right.Denominator);

    /// <summary>Multiplies two exact rational values.</summary>
    public static ExactRational operator *(ExactRational left, ExactRational right) =>
        new(left.Numerator * right.Numerator, left.Denominator * right.Denominator);

    /// <summary>Determines whether two reduced rational values are equal.</summary>
    public bool Equals(ExactRational other) =>
        Numerator == other.Numerator && Denominator == other.Denominator;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ExactRational other && Equals(other);
    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);
    /// <inheritdoc />
    public override string ToString() => $"{Numerator}/{Denominator}";
    /// <summary>Determines whether two exact rational values are equal.</summary>
    public static bool operator ==(ExactRational left, ExactRational right) => left.Equals(right);
    /// <summary>Determines whether two exact rational values are not equal.</summary>
    public static bool operator !=(ExactRational left, ExactRational right) => !left.Equals(right);
}
