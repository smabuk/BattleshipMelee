using StronglyTypedIds;

[StronglyTypedId(backingType: StronglyTypedIdBackingType.String, converters: StronglyTypedIdConverter.TypeConverter | StronglyTypedIdConverter.SystemTextJson)]
public partial struct ConnectionId { }
