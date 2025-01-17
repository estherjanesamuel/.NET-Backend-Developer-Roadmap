## Setting the options for JsonSerializer.SerializeAsync

These are the following options (JsonSerializerOptions) you can set that affect the serialization of your object.

- IgnoreNullValues. If your property is null, do not serialize it to JSON
- PropertNamingPolicy. If you don't set this, yout property will be serialized as PascalCase, which is the property naming convention for C#. To serialize it camelCase, use JsonNamingPolicy.CamelCase.
- DictionaryKeyPolicy. Ditto above but for dictionary keys.
- WriteIndented. Set it to true so it is formatted for human readibility.
For production though, set it to false.