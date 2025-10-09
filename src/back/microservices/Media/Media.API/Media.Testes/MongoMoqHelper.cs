using Moq;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public static class MongoMoqHelper
{
    public static Mock<IAsyncCursor<TDocument>> MockCursor<TDocument>(IEnumerable<TDocument> documents)
    {
        var mockCursor = new Mock<IAsyncCursor<TDocument>>();
        mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                  .Returns(true)
                  .Returns(false);
        mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                  .Returns(Task.FromResult(true))
                  .Returns(Task.FromResult(false));
        mockCursor.SetupGet(_ => _.Current).Returns(documents);
        return mockCursor;
    }
    public static Mock<IFindFluent<TDocument, TDocument>> MockFindFluent<TDocument>(IEnumerable<TDocument> documents)
    {
        var mockFind = new Mock<IFindFluent<TDocument, TDocument>>();
        var cursor = MockCursor(documents);
        mockFind.Setup(f => f.ToCursorAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(cursor.Object);

        TDocument? firstOrDefaultResult = documents.FirstOrDefault();

        mockFind.Setup(f => f.FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(firstOrDefaultResult); 

        mockFind.Setup(f => f.ToListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(documents.ToList());

        mockFind.Setup(f => f.Limit(It.IsAny<int?>())).Returns(mockFind.Object);
        mockFind.Setup(f => f.Skip(It.IsAny<int?>())).Returns(mockFind.Object);

        return mockFind;
    }
}