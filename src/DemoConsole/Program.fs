open System
open System.Text.RegularExpressions

let square x = x * x
let isOdd x = x % 2 <> 0

let getSquaresOfOdds nums =
    nums |> List.filter isOdd |> List.map (fun x -> (x, square x))

[<EntryPoint>]
let main argv = 
    printfn "Hello, world!  Square of %d is %d" 12 (square 12)

    let numers = [ 1 .. 25 ]
    let squaresOfOdds = getSquaresOfOdds numers

    for (num, square) in squaresOfOdds do
        printfn "%d squared is %d" num square

    let message = "Seth is super awesome and he is fun to talk to"

    let superCount = Regex.Matches(message, "super").Count

    printfn "%d" superCount

    Console.Read() |> ignore
    0 // return an interger exit code