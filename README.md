# Easy

Easy are C# libraries for storing compressed data.

## EasyLZ
EasyLZ is a compression algorithm based on LZ77 that uses a byte to store the length and distance of the copy in the dictionary.

The compression flow occurs with a byte code that each bit (from largest to smallest) informs whether the next byte of the flow is literal or copies.

Examples:
```csharp
byte code = 0b11111111; // next 8 bytes of the stream are literal
```

```csharp
byte code = 0b00000000; // next 8 bytes of the stream are copies
```

```csharp
byte code = 0b11111110; // of the next 8 bytes of the stream, the first 7 are literal and the last is a copy
```

#### Compression
```csharp
byte[] compressed = new byte[EasyLZ.MaxLengthRawEncode(data.Length)];
int length = EasyLZ.Encode(data, 2, compressed);
```
#### Decompression
```csharp
byte[] decompressed = EasyLZ.Decode(data);
```
### Literal byte
A literal byte is an uncompressed byte of the stream that must be copied to the output stream.

### Copy byte
A copy byte is a compressed byte divided into two-bit regions.

The first region + 2 stores the size of the compressed bytes (copy size) and the second part + 1 stores the distance of the output stream to the first byte that will be copied.

The delimiter of what is the first part and second part is the first byte of the stream.

Examples:
```csharp
byte firstByte = 2; // first 2 most significant bits of the byte are for length and the other 6 bits for offset
byte copyByte = 0b01000001 // copies 3 bytes from 2 offset backwards from the current output stream to the output stream
```

## EasyBitmap
A bitmap format is simple but can be compressed with Deflate, LZ4, RLE and EasyLZ.

>
       P  |         Values      |       Purpose                             
       0  | 0x45 0x53 0x42 0x4D | ESBM in ASCII, identify the format easily.
       3  | 0x0D 0x0A           | Newline
       5  | 0x03                | End-of-text charecter(ETX)
          |
          | ----------- Info header -----------
          |      Bytes
       6  |        8            | File size
      14  |        4            | Width
      18  |        4            | Height
      22  |        1            | Pixel order:
          |                         0 = RGBA
          |                         1 = ARGB(Default)
          |                         2 = GrayScale
          |                         3 = GrayScale-A
      23  |        4            | Compression:
          |                         0 - No compression
          |                         1 - Deflate
          |                         2 - LZ4
          |                         3 - RLE
          |                         4 - EasyLZ
      27  |        4            | Filter:
          |                         0 - No filter
          |                         1 - Axis
          |                         2 - Sub
      31  |        8            | Size of image data in bytes without compression.
            ----------- Image data -----------
      39        IMAGE DATA 

### Filter
Before compression, it is also possible to choose between Axis and Sub filters to modify the way bytes are saved, in order to reduce pixel entropy and improve compression.

#### Sub
The Sub filter turns the current byte into the offset between it and the previous byte. 
> 0xFF, 0xFF, 0xFF, 0x00\
> turns,\
> 0xFF, 0x00, 0x00, 0x01

#### Axis
The standard way to store an image's bytes is to record the image's horizontal pixels in the stream, so that compression occurs only if the horizontal bytes are repeated, ignoring the repeated bytes of the vertical axis.

The Axis filter reads the line(horizontal) and the column(vertical) and goes down which one to write in the flow using a function that calculates a note that says how much there is no entropy; after descending, it adds a byte that indicates whether the row was horizontal or vertical and in sequence the bytes of the row or column filtered with Sub.

### EasyBitmap.GDI
EasyBitmap.GDI has extensions to transform a System.Drawing.Bitmap object into Easy.EasyBitmap and vice versa.

### EasyBitmap.PaintNET
An add-on for the Paint.NET image editor to support the image format.

## EasyArchive
EasyArchive is a format for archiving files and whether or not to compress each file individually.\
Supports compression with Deflate, LZ4 or EasyLZ.

>
        (numbers is in little endian)

        ESAR - identifies EasyArchive(ASCII)
        8 bytes - number of files

        for each file:
            2 bytes - compression
            4 bytes - name file size in bytes(N)
            N bytes - file name(UTF8)
            8 bytes - position in file
            8 bytes - last write time(unix time)
            8 bytes - uncompressed size
            8 bytes - compressed size
            1 byte  - PearsonHashing in compressed data
            1 byte  - PearsonHashing in uncompressed data

        *data file*
## License
[MIT](https://choosealicense.com/licenses/mit/)
