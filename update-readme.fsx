#r "nuget: FSharp.Data"

open System.IO
open System.Text.RegularExpressions
open FSharp.Data

let currentDirectory = System.Environment.CurrentDirectory
let readmeLocation = Path.Join(currentDirectory, "README.asciidoc")

let replaceTextInGroup (currentText: string) replaceText (group: Group) = 
    currentText.Replace(group.Value, $"{System.Environment.NewLine}{replaceText}{System.Environment.NewLine}")

type Untappd = JsonProvider<"checkin-sample.json", ResolutionFolder = __SOURCE_DIRECTORY__>

let checkins fromDirectory= 
    Untappd.Load(Path.Join(fromDirectory, "untappd/beers.json"))

let defaultPhoto img =
    img |> Option.defaultWith (fun () -> "")

let formatCheckins =
    checkins currentDirectory
    |> Array.map (fun checkin -> sprintf $"| image:{checkin.Photo |> defaultPhoto}[{checkin.Name} - {checkin.Brewery}] | Drinking a *{checkin.Name}* by *{checkin.Brewery}*. It is a *{checkin.Style}* style of beer. | I rated it a *{checkin.Rating}*")
    |> Array.reduce (fun i acc -> $"{i}{System.Environment.NewLine}{acc}")

let readme = File.ReadAllText(readmeLocation)
let updatedReadme = Regex.Match(readme, "(?:\/\/ untappd beer)(?<beers>[\w\s\S]+)(?:\/\/ untappd end)").Groups
                    |> Seq.find (fun grp -> grp.Name = "beers")
                    |> replaceTextInGroup readme formatCheckins

File.WriteAllText (readmeLocation, updatedReadme, System.Text.Encoding.UTF8)
