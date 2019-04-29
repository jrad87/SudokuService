namespace Solver

module RecursiveBacktrackingSolver =
    let row index = index / 9
    let col index = index % 9
    let index r c = 9 * r + c     
    let blockRange n = 3 * (n / 3)       
    let checkGroup value group = group |> List.exists (fun x -> x = value) |> not
    let checkRow (arr: int[]) r value = [for c in 0 .. 8 -> arr.[index r c]] |> checkGroup value
    let checkCol (arr: int[]) c value = [for r in 0 .. 8 -> arr.[index r c]] |> checkGroup value
    let checkSector (arr: int[]) r c value =
        let rowStart, colStart = (blockRange r), (blockRange c)
        [for _r in rowStart .. rowStart + 2 do 
            for _c in colStart .. colStart + 2 -> arr.[index _r _c]] |> checkGroup value 
    let isValidPlacement arr row col value = 
            checkRow arr row value &&
            checkSector arr row col value &&
            checkCol arr col value 
    let rec backtrack (grid: int[]) cont i value =    
        if i = 81 then true
        elif grid.[i] <> 0 then backtrack grid cont (i + 1) 1
        else
            if isValidPlacement grid (row i) (col i) value 
            then
                grid.[i] <- value
                if backtrack grid cont (i + 1) 1 then true else grid.[i] <- 0; cont grid i value
            else
                cont grid i value
    let rec nextValue grid i value =
        if (value + 1) < 10 then backtrack grid nextValue i (value + 1) else false
    let inline charToInt c = int c - int '0'
    let solve (puzzle: char[]) =
        let p = puzzle |> Array.map charToInt
        backtrack p nextValue 0 1 |> ignore
        p 