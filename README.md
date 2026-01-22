# VaporDeobfuscator

Simple deobfuscator for .NET assemblies protected with **VaporObfuscator**.

Target framework: **.NET Framework 4.7.2**  
Library: **dnlib**

---

## Features

- Removes `ObfuscatedByVapor` module name watermark
- Removes fake attributes and junk types
- Removes anti-de4dot `FormXXX` types
- Decrypts Base64 string protection
- Saves output as `.unpacked.exe` / `.unpacked.dll`

---

## Limitations

- Original names cannot be restored
- Supports only Vapor-style protection
- Not a universal deobfuscator

## Usage

```
VaporDeobfuscator.exe target.exe
```

Output:

```
target.unpacked.exe
```
## License

MIT
