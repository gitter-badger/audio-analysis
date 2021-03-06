module QutSensors.AudioAnalysis.AED.Util

open Microsoft.FSharp.Math.SI
open System.Text
open System.IO
open Microsoft.FSharp.Math
open Microsoft.FSharp
open SixLabors.ImageSharp

// If the first Option is not empty return it, else return the second. Copy of Scala Option.orElse.
let orElse o (p:'a option Lazy) = if Option.isSome o then o else p.Force()

let (|?) = orElse

let (|?|) = defaultArg

let (|?>) item (condition, f) = if condition then f item else item

// Haskell catMaybes
let catOptions xs = Seq.filter Option.isSome xs |> Seq.map Option.get

let uncurry f (x,y) = f x y

let floatEquals (f1:float) f2 d = abs(f1 - f2) <= d

let roundUpTo v x = if x < v then v else x
let roundDownTo v x = if x > v then v else x
let inline round (x: ^a) = round x
let inline sqr x = x*x    



let (><) x (l,u) = x > l && x < u // in open interval
let (>==<) x (l,u) = x >= l && x <= u // in closed interval


let boundedInterval (p:float<_>) ld up lb ub = (p-ld |> roundUpTo lb, p+up |> roundDownTo ub)

let maxmap f = Seq.max << Seq.map f
let minmap f = Seq.min << Seq.map f

let sumRows (m:matrix) = Matrix.foldByRow (+) (Math.Vector.zero m.NumRows) m
let sumColumns (m:matrix) = let v = Math.Matrix.foldByCol (+) (Math.RowVector.zero m.NumCols) m in v.Transpose

// TODO this is now in PowerPack.Compatibility (need to add specific Reference)
let split (d:char array) (s:string) = s.Split(d, System.StringSplitOptions.RemoveEmptyEntries)

let array2Dfold f z (a:'a[,]) =
     let mutable x = z
     for i=0 to (a.GetLength(0)-1) do
       for j=0 to (a.GetLength(1)-1) do
         x <- f x (a.[i,j])
       done
     done
     x

let mTranspose = Math.Matrix.transpose

let mapByCol f (m:matrix) = seq{m.NumCols-1..-1..0} |> Seq.fold (fun z j -> f (m.Column j) ::z) [] |> Math.Vector.Generic.ofList

let mapByRow f (m:matrix) = seq{m.NumRows-1..-1..0} |> Seq.fold (fun z i -> f (m.Row i) ::z) [] |> Math.Vector.Generic.ofList

// Assume matricies m,n are exactly same dimensions
let matrixMap2 f (m:matrix) (n:matrix) = Math.Matrix.init m.NumRows m.NumCols (fun i j -> f m.[i,j] n.[i,j])

// Assume matricies m,n,o are exactly same dimensions
let matrixMap3 f (m:matrix) (n:matrix) (o:matrix) = Math.Matrix.init m.NumRows m.NumCols (fun i j -> f m.[i,j] n.[i,j] o.[i,j])

let matrixMapi2Unzip f (m:matrix) =
    let r = Math.Matrix.zero m.NumRows m.NumCols
    let s = Math.Matrix.zero m.NumRows m.NumCols
    let mutable x = (0.0, 0.0)
    for i=0 to (m.NumRows-1) do
      for j=0 to (m.NumCols-1) do
        x <- f i j m.[i,j]
        let (y,z) = x
        r.[i,j] <- y
        s.[i,j] <- z
      done
    done
    (r,s) 

/// A unit of measure for a Pixel
[<Measure>] type px
let px = 1.0<px>

type Pixelf = float<px>
type Pixel = int<px>

[<Measure>] type percent
type Percent = float<percent>  
let unit p = p / 100.0<percent>
let percent u : Percent = u * 100.0<percent>

type System.Double with
    member x.percent = x * 1.0<percent>
    member x.toPercent = percent x
    member x.toUnit = unit x
    
let inline s x y = x - y
//type 'a Rectangle = {Left:'a; Top:'a; Right:'a; Bottom:'a; Width:'a; Height:'a;}
[<StructuredFormatDisplayAttribute("Rectangle: L:{Left}, R:{Right}, T:{Top}, B:{Bottom}")>]
type Rectangle<'a, 'b> = 
    {Left:'a; Top:'b; Right:'a; Bottom:'b; }
    override m.ToString() = sprintf "Rectangle: L:{%A}, R:{%A}, T:{%A}, B:{%A}" m.Left m.Right m.Top m.Bottom
        

type Rectangle2<'a> = Rectangle<'a, 'a>


//type RectangleF<[<Measure>]'b, [<Measure>]'c> = {Left:float<'b>; Top:float<'c>; Right:float<'b>; Bottom:float<'c>; Width:float<'b>; Height:float<'c>;}
//type Rectangle<[<Measure>]'b, [<Measure>]'c> = {Left:int<'b>; Top:int<'c>; Right:int<'b>; Bottom:int<'c>; Width:int<'b>; Height:int<'c>;}

//let r = {Left=3.0<m>; Top=6.0<s>; Right=5.0<m>; Bottom=2.0<s>; Width=3.0<m>; Height=9.0<s>;}
//let r = {Left=3.0; Top=6.0; Right=5.0; Bottom=2.0; Width=3.0; Height=9.0;}

let inline addDimensions  (convertA:'c) (convertB:'d) (r:Rectangle<'a,'b>) : Rectangle<'c,'d> = 
    {
        Left= r.Left * convertA;
        Top= r.Top * convertB;
        Right= r.Right * convertA;
        Bottom= r.Bottom * convertB;
    }

let inline removeDimensions (r:Rectangle<'a,'b>) (convertA:'a) (convertB:'b) : Rectangle<'c,'d> = 
    {
        Left= r.Left / convertA;
        Top= r.Top / convertB;
        Right= r.Right / convertA;
        Bottom= r.Bottom / convertB;
    }
    
type EventRect = Rectangle<float<s>, float<Hz>>
type pxf = float<px>

let inline cornersToRect l r t b = {Left=l; Top=t; Right=r; Bottom=b}
let inline lengthsToRect l t w h = 
    if w < 0 then
        failwith "Width cannot be less than zero"
    if h < 0 then
        failwith "Height cannot be less than zero"
    if t < 0 then
        failwith "top cannot be less than zero"
    if l < 0 then
        failwith "left canot be less than zero"
    {Left=l; Top=t; Right=l+w-1; Bottom=t+h-1}
let inline cornersToRect2 (l, r) (t, b) = cornersToRect l r t b
let fcornersToRect (l:float) r (t:float) b = cornersToRect l r t b // for C#
let inline left r = r.Left
let inline right r = r.Right
let inline top r = r.Top
let inline bottom r = r.Bottom
let inline bottomLeft r = (r.Left, r.Bottom)

let inline increment n = n + LanguagePrimitives.GenericOne
let inline width r = (right r) - (left r) |> abs |> increment
let inline width2 (right:float<_>) (left:float<_>) = right - left |> abs |> (+) (LanguagePrimitives.FloatWithMeasure 1.0)
// for event pattern recog compatibility
let inline oldWidth r = (right r) - (left r)

let inline height r = (top r) - (bottom r) |> abs |> increment
let inline height2 (top:float<_>) (bottom:float<_>) = top - bottom |> abs |> (+) (LanguagePrimitives.FloatWithMeasure 1.0)
let inline area r = (width r) * (height r)
let inline areaUnits r = (area r) * 1<px^2>
let inline isWithin r (x,y) =
    not (x < r.Left || x > r.Right || y < r.Top || y > r.Bottom)
    //x >= r.Left && x < r.Right && y >= r.Top && y < r.Bottom
let inline isWithin2 r (y,x) =
    isWithin r (y, x)

let inline toFloatRect r =
    cornersToRect (left r |> float) (right r |> float) (top r |> float) (bottom r |> float)

let inline toPoint (x, y) = new Point(x, y)
let inline toPoint2 (y, x) = new Point(x, y)
    
(* This is currently done the easy, inefficient way.

   The following Matlab code will write the matrix I1 to the file I1.txt, with one element per line
   by descending each column in turn.
   
    fid = fopen('I1.txt', 'wt');
    fprintf(fid, '%f\n', I1);
    fclose(fid);
 *)
let fileToMatrix f r c =
    let ls = System.IO.File.ReadAllLines f
    let a = Array2D.create r c 0.0
    Array.iteri (fun i (s:string) -> a.[i % r, i / r] <- System.Convert.ToDouble(s)) ls
    a
    
let csvToMatrix f =
    let ls = System.IO.File.ReadAllLines f
    let xs = Array.map (split [|','|]) ls    
    let m = Math.Matrix.init (ls.GetLength 0) (Array.length xs.[0]) (fun i j -> System.Convert.ToDouble(xs.[i].[j]))
    m
    
// TODO this is very slow
let matrixToCsv (m:matrix) f =
    let g i = m.Row i |> Seq.map (sprintf "%A") |> String.concat ","
    System.IO.File.WriteAllLines(f, Array.init m.NumRows g)

let itemsToFile path (transformer: 'a -> string) items = 
    use file = File.CreateText(path)
    Seq.iter (fun x ->  x |> transformer |> file.WriteLine) items

