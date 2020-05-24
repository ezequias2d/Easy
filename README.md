# Easy

Easy are C # libraries for storing compressed data.

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
