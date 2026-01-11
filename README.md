HOW TO RUN:

1. Open Solution via prefferable code Editor or IDE
2. Launch API project under https profile. You should see your app running on port 7025. (If it's other port replace url in comman in step 4.)
3. Go to TradeWeb\TradeWeb.API folder, open Terminal there
4. Paste following
   curl.exe --data-binary "@Files/Data/middleSizeTrade.csv" -H "Content-Type: text/csv" https://localhost:7025/api/v1/enrich
5. Wait for process to complete, results will be outputed to terminal

What can be improved:

1. Domain layer can be used to parse data before outputing it to PipeWriter
2. Various format implementation, ITradeEnrichmentService can have multiple implementations for Csv, Json, Xml etc.
3. Improved logging and Exception handling, via MediatR Pipeline behaviour
4. Async processing/parallelism data can be splitted to chunks that can be processed independently in parrallel than combined back into one
