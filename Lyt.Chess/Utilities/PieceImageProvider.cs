namespace Lyt.Chess.Utilities;

public static class PieceImageProvider
{
    private const int ColorCount = 2;
    private const int PieceCount = 6;
    private const string DefaultResourceName = "MeridaChessPieces.png";
    public static Dictionary<char, CroppedBitmap> PieceImages { get; private set; }
    private static string ResourceName;
    private static readonly char[] PiecesKeys = ['K', 'Q', 'B', 'N', 'R', 'P', 'k', 'q', 'b', 'n', 'r', 'p'];

    static PieceImageProvider()
    {
        ResourceName = string.Empty;
        PieceImages = new(12);
    }

    public static void Inititalize(string resourceName = "")
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            resourceName = DefaultResourceName;
        }

        if (ResourceName == resourceName)
        {
            return;
        }

        ResourceName = resourceName;
        PieceImages.Clear();

        ResourcesUtilities.SetResourcesPath("Lyt.Chess.Resources.PiecesSpriteSheets");
        ResourcesUtilities.SetExecutingAssembly(Assembly.GetExecutingAssembly());
        byte[] imageBytes = ResourcesUtilities.LoadEmbeddedBinaryResource(resourceName, out string? _);
        var bitmap = 
            ImagingUtilities.DecodeBitmap(imageBytes) ?? 
            throw new InvalidOperationException("Could not load piece images.");
        double pieceImageWidth = bitmap.PixelSize.Width / (double)PieceCount;
        double pieceImageHeight = bitmap.PixelSize.Height / (double)ColorCount;
        int roundedWidth = (int)Math.Ceiling(pieceImageWidth);
        int roundedHeight = (int)Math.Ceiling(pieceImageHeight);

        for (int colorI = 0; colorI < 2; colorI++)
        {
            for (int pieceI = 0; pieceI < 6; pieceI++)
            {
                int x = (int)Math.Ceiling(pieceI * pieceImageWidth);
                int y = (int)Math.Ceiling(colorI * pieceImageHeight);
                char key = PiecesKeys[colorI * 6 + pieceI];
                var roi = new PixelRect(x, y, roundedWidth, roundedHeight);
                var croppedBimap = new CroppedBitmap(bitmap, roi);
                PieceImages.Add(key, croppedBimap);
            }
        }
    }

    public static CroppedBitmap GetFromFen(char FenCharacter)
    {
        if (PieceImages is null || PieceImages.Count == 0)
        {
            Inititalize(DefaultResourceName);
        }

        if (PieceImages is not null &&
            PieceImages.TryGetValue(FenCharacter, out CroppedBitmap? croppedBitmap) &&
            croppedBitmap is not null)
        {
            return croppedBitmap;
        }

        throw new ArgumentException("Invalid FEN character.");
    }
}
