# Task 1: Data Structure Problem Solving

Here is my approach and architectural design for the type-ahead search feature over 1 million patient records.

### 1. Which data structure and matching algorithm would you choose for this autocomplete use-case, and why?

For a dataset of 1 million records, my go-to solution would be an in-memory inverted index using **Redis**, specifically paired with the **RediSearch** module.

For the matching algorithm, I would configure RediSearch to use **Edge N-Gram tokenization**. This means when a patient named "Jonathan" is saved, the engine automatically breaks the name down into prefix chunks like "jo", "jon", and "jona". When a user searches, the engine performs a direct dictionary lookup against those tokens in memory. RediSearch also natively handles relevance scoring (prioritizing exact matches using BM25) and typo tolerance, so if a doctor misspells "Jonothan", the right patient still shows up.

**Why this approach?** 
I'd avoid using a standard SQL query (like `LIKE '%query%'`) because it causes full-table scans that will eventually choke the primary database under heavy load. Furthermore, 1 million records is actually quite small—roughly 50MB of raw text. Even after the N-Grams expand the data size, the entire search index will comfortably fit into 300MB to 500MB of RAM. Keeping the search index entirely in memory avoids disk I/O completely, making it the most pragmatic and performant choice for this scale.

---

### 2. What is the expected performance of your solution, both in terms of algorithmic time complexity and real-world query latency?

Algorithmically, the time complexity for the lookup is essentially **$O(1)$** because the prefix tokens are pre-computed during indexing. It just takes a direct hash map lookup to find the token, followed by $O(K \log K)$ to sort the top $K$ results by relevance.

In terms of real-world query latency, because Redis operates strictly in-memory, the backend execution time is incredibly fast—usually between **1 and 3 milliseconds**. This leaves a massive 85-95 millisecond buffer purely for the user's internet connection. It guarantees we will comfortably meet the strict sub-100ms requirement without breaking a sweat.

---

### 3. What specific tools or technologies would you leverage to implement and optimize this design?

For the core stack, I'd use **SQL Server** as the primary database (the source of truth), **Redis** for the search engine, and **ASP.NET Core Minimal APIs** for the backend to keep things lightweight.

Here is how I would optimize the architecture:

**Data Synchronization:**
Since Redis is volatile, it shouldn't be the master record. To keep it synced with SQL, I would use the **Outbox Pattern**. When a patient is saved in SQL, we write a log to an `OutboxEvents` table in the exact same database transaction. A background .NET worker then reads that table every second and pushes the new data to Redis. This ensures the primary database is never bogged down by search indexing.

**Disaster Recovery:**
If the Redis server crashes, the RAM clears. To mitigate this, I'd enable Redis persistence (AOF/RDB snapshots) so the index restores itself on reboot. As a failsafe, we can build a "Full Re-Sync" API endpoint. If the server is permanently lost, we can rebuild the entire index from the SQL master data in seconds. During any downtime, the API would gracefully fall back to a slower SQL query so the system doesn't break.

**Front-End Protections:**
None of this backend speed matters if the front-end spams the API. The UI must implement:
*   **Debouncing:** A strict 300ms delay after the user stops typing before making the API call.
*   **AbortController:** Canceling older in-flight HTTP requests if the user continues typing, which prevents UI race conditions caused by laggy internet.
*   **Thresholds:** Requiring at least 2-3 characters before searching, and limiting the UI dropdown to 10 results to prevent browser rendering lag.

***Note on Scalability:*** *Redis is the fastest and most practical choice for 1 million records. However, if the hospital eventually scales to tens of millions of patients, storing the entire expanded index in RAM will become expensive. At that point, it would make business sense to migrate the search engine to Elasticsearch, which stores the bulk of the index on cheaper SSD storage while still maintaining sub-50ms latency.*
