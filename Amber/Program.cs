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

Console.WriteLine("Hello World");
