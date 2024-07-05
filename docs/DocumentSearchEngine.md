# Document Search

## Summary

This is a simplified design for an semantic search engine

## Actions

### Upload File Process

```plantuml
actor User
boundary DocumentController as Controller
collections ResourceContainer as Container
queue ResourcesQueue as Queue

User -> Controller: Upload(...)
activate Controller
    Controller -> Container : StoreContent(...)
    activate Container
        Container -\ Queue : id
        return id
    return id
```

### File Uploaded (Resource Handler)

```plantuml
actor System
boundary ResourceHandler as Handler
collections ResourceContainer as Source
control DocumentConverter as Converter
collections TextResourceContainer as Target
queue TextResourcesQueue as Queue

System -\ Handler: HandleAsync(id)
activate Handler
    Handler -> Source : GetContent(id)
    activate Source
        return content
    Handler -> Converter : Convert(content)
    activate Converter
        return converted
    Handler -> Target : Store(converted)
    activate Target
        return stored-id
    Handler -\ Queue : Send(stored-id)
```

### File Converted (Text Resource Handler)

```plantuml
actor System
boundary TextResourceHandler as Handler
collections TextResourceContainer as Source
control IMessageCompletion as Summarizer
collections SummariesContainer as Target
queue IndexerQueue as Queue

System -\ Handler: HandleAsync(id)
activate Handler
    Handler -> Source : GetContent(id)
    activate Source
        return content
    Handler -\ Queue : Send(id, 'textResource')

    Handler -> Summarizer : Summarize(content)
    activate Summarizer
        return summarized

    Handler -> Target : Store(summarized)
    activate Target
        return summarized-id

    Handler -\ Queue : Send(summarized-id, 'summarized')
```

### Content Indexer (Index Handler)

```plantuml
actor System
boundary TextResourceHandler as Handler
collections Container as Container
control IEmbeddingProvider as Embedding
database DocumentVectors as Vectors

System -\ Handler: HandleAsync(id)
activate Handler
    Handler -> Handler : SelectContainer(type)
    activate Handler
        return container

    Handler -> Container : GetContent(id)
    activate Container
        return content
        
    Handler -> Handler : Split(content)
    activate Handler
        Handler -> Embedding : GetEmbedding(content-chunk)
        activate Embedding
            return embedding

        Handler -> Vectors : StoreVectors(embedding, metaData)
        activate Vectors
            return embedding-id

        return embedding-id[]
        
        Handler -> Container : StoreData(id, embedding-id[], metaData)
        activate Container
            return 
```

### Content Search 

```plantuml
actor User
boundary VectorController as Controller
control IEmbeddingProvider as Embedding
database DocumentVectors as Vectors

User -> Controller : Search(query-text)
activate Controller
    Controller -> Embedding : GetEmbedding(query-text)
    activate Embedding
        return embedding

    Controller -> Vectors : ListGrouped(orderBy:embedding, groupBy:id)
    activate Vectors
        return metadata[]

    return metadata[]
```

## Controllers

```plantuml
class DocumentController {
    + List() : ContentMetaDataReference[]
    + Download(file:string) : FileStreamResult
    + Delete(file:string) : IActionResult
    + Upload(content:IFileForm, override:bool) : IActionResult
    + Store(model:ContentMetaDataReference) : IActionResult
}

class SummariesController {
    + List() : ContentMetaDataReference[]
    + Download(file:string) : FileStreamResult
    + Delete(file:string) : IActionResult
    + Store(model:ContentMetaDataReference) : IActionResult
}

class TextDocumentController {
    + List() : ContentMetaDataReference[]
    + Download(file:string) : FileStreamResult
    + Delete(file:string) : IActionResult
    + Store(model:ContentMetaDataReference) : IActionResult
}

class VectorController {
    + List() : SearchResultModel[]
    + Query(embedding:float[]) : SearchResultModel[]
    + QueryGrouped(embedding:float[], groupedBy:string) : SearchResultModel[]
    + Search(query:string) : SearchResultModel[]
    + SearchGrouped(query:string, groupedBy:string) : SearchResultModel[]
}
```

## Entity Diagram

```plantuml
entity ResourceProfilerContext {
    + Documents : DbSet<Document>
    + Vectors : DbSet<DocumentVector>
    + MessageQueue : DbSet<MessageQueue>
}

entity Document {
    + Id : int, identity
    + FileName : string
    + Hash : string
    + ContentType: string
    + ContainerName: string
    + Content: byte[]
}

entity DocumentData {
    + Id : int, identity
    + Name : string
    + Value : string
}

entity DocumentVector {
    + Id : int, identity
    + Embedding : vector
}

entity MessageQueue {
    + Id : int, identity
    + MessageType : string
    + ChannelType : string
    + CorrelationId : string
    + SentAt : DateTimeOffset
    + SentFrom : string
    + SentBy : string
    + SentId : string
    + Content : string
    + QueueName : string
}

Document --> DocumentData : Data
Document --> DocumentVector : Vectors
DocumentVector --> DocumentData : Data

ResourceProfilerContext --> Document : Documents
ResourceProfilerContext --> DocumentVector : Vectors
ResourceProfilerContext --> MessageQueue : MessageQueue
```

## Database

```plantuml
object Document {
    *Id : int, identity
    FileName : string
    Hash : string
    ContentType: string
    ContainerName: string
    Content: byte[]
}

object DocumentData {
    *Id : int, identity
    Name : string
    Value : string
}

object DocumentVector {
    *Id : int, identity
    Embedding : vector(768)
}

object MessageQueue {
    *Id : int, identity
    MessageType : string
    ChannelType : string
    CorrelationId : string
    SentAt : DateTimeOffset
    SentFrom : string
    SentBy : string
    SentId : string
    Content : string
    QueueName : string
}

Document *-{ DocumentData : Data
Document *-{ DocumentVector : Vectors
DocumentVector *-{ DocumentData : Data
```