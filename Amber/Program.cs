// workspace
// -- contains many documents
// -- manages lazy loading of documents, implement lazy loading with json documents
// -- keeps log of deleted documents forever

// document
// -- has a type
// -- keeps log of mutations, also session based
// -- is rebuilt every time the mutation log changes
// -- can be deleted
// -- knows how to apply mutations to it

// special remarks
// -- do not list every available document
// -- allow cold fetching with type and guid
// create own store for every document type


var sessionId = Guid.Parse("794dcb19-a00e-4f5a-9eeb-5a2d3b582f60");



Console.WriteLine("Hello World");
