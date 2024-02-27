using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Net.WebRequestMethods;

namespace lab4_pre_alpha {
  class Memento {

    public string savedPath;
    public string changePath {
      get { return savedPath; }
      set { savedPath = value; }
    }

    public string savedText;
    public string changeText {
      get { return savedText; }
      set { savedText = value; }
    }
  }
  public interface IOriginator {
    object getMemento();
    void setMemento (object memento);
    
   }
  [Serializable]
  public class TextFile : IOriginator {
    public string pathOfFile;
    public string textInFile;
    public FileStream textFileStream;
    public byte[] fileData;
    public TextFile() { }
    public TextFile (string path) {
      pathOfFile = path;
      textFileStream = new FileStream(pathOfFile, FileMode.OpenOrCreate);
      byte[] temp = new byte[textFileStream.Length];

      textFileStream.Read(temp, 0, temp.Length);
      fileData = temp;

      textFileStream.Close();
    }
    
    private void GetText() {
      textFileStream = new FileStream(pathOfFile, FileMode.OpenOrCreate);
      byte[] temp = new byte[textFileStream.Length];

      textFileStream.Read(temp, 0, temp.Length);
      fileData = temp;

      textFileStream.Close();
    }
    public void ShowText () {
      GetText();
      textInFile = Encoding.Default.GetString(fileData);
      Console.WriteLine(textInFile);
    }

    public void WriteText(string text) {
      System.IO.File.AppendAllText(pathOfFile, text);
    }

    public void Close () { 
      textFileStream.Close();
    }
    public void BinarySerialize(FileStream textFileStream) { 
      BinaryFormatter binFormatter = new BinaryFormatter();

      binFormatter.Serialize(textFileStream, this);
      textFileStream.Flush();
      textFileStream.Close();
    }
    public void BinaryDeserialize(FileStream textFileStream) {
      BinaryFormatter binFormatter = new BinaryFormatter();
      TextFile deserealised = (TextFile)binFormatter.Deserialize(textFileStream);

      pathOfFile = deserealised.pathOfFile;
      textInFile = deserealised.textInFile;
      fileData = deserealised.fileData;
      
      textFileStream.Close();
    }
    public void XMLSerialize(FileStream textFileStream) {
      XmlSerializer xmlSerialize = new XmlSerializer(typeof(TextFile));
      xmlSerialize.Serialize(textFileStream, this);

      textFileStream.Flush();
      textFileStream.Close();
    }
    public void XMLDeserialize(FileStream textFileStream) {
      XmlSerializer xmlSerialize = new XmlSerializer(typeof(TextFile));
      TextFile deserealised = (TextFile)xmlSerialize.Deserialize(textFileStream);

      pathOfFile = deserealised.pathOfFile;
      textInFile = deserealised.textInFile;
      fileData = deserealised.fileData;

      textFileStream.Close();
    }
    object IOriginator.getMemento() {
      textFileStream = new FileStream(pathOfFile, FileMode.OpenOrCreate);
      byte[] temp = new byte[textFileStream.Length];

      textFileStream.Read(temp, 0, temp.Length);
      fileData = temp;

      textFileStream.Close();
      textInFile = Encoding.Default.GetString(fileData);
      Console.WriteLine("File saved");
      return new Memento() {
        savedPath = pathOfFile, savedText = textInFile
      };
    }
    void IOriginator.setMemento(object memento) {
      if (memento is Memento) {

        System.IO.File.Delete(pathOfFile);

        var newMemento = memento as Memento;
        pathOfFile = newMemento.savedPath;
        textInFile = newMemento.savedText;
        FileStream replaceText = new FileStream(pathOfFile, FileMode.OpenOrCreate);
        StreamWriter writer = new StreamWriter(replaceText);
        writer.Write(textInFile);
        writer.Close();
        replaceText.Close();
        Console.WriteLine("File restored");
      }
    }
    public string SetPath {
      get { return pathOfFile; }
      set { pathOfFile = SetPath; }
    }


  }
  class WordFinder {
    private string[] filesWithWords;
    private string keywords;
    private string innerPath;
    private string textInFile;
    private int innerNumberOfWords;
    public WordFinder ( string inputKeywords, string path) {
      innerPath = path;
      keywords = inputKeywords;
    }
    public void Search () {
      string[] directory = Directory.GetFiles(innerPath);
      bool notFound = true;
      bool noWordInFile = true;

      foreach (string filename in directory) {
        FileStream searchFileStream = new FileStream(filename, FileMode.OpenOrCreate);
        byte[] temp = new byte[searchFileStream.Length];

        searchFileStream.Read(temp, 0, temp.Length);
        textInFile = Encoding.Default.GetString(temp);

        if (textInFile.Contains(keywords)) {
          notFound = false;
          noWordInFile = false;
        }
        if (!notFound) {
          Console.WriteLine("Contain keyword in " + filename);
          notFound = true;
        }
        if (noWordInFile) {
          Console.WriteLine("File " + filename + " does not contain keyword " + keywords);
        }

        searchFileStream.Close();
      }     
    }
  }
  public class Caretaker {
    private object memento;
    public void SaveState (IOriginator originator) {
      memento = originator.getMemento();
    }
    public void RestoreState (IOriginator originator) {
      originator.setMemento(memento);
    }
  }

  internal class Program {
    static void Main(string[] args) {
      string path = "some path";
      string text;
      bool UserDoesNotWantExit = true;
      char select = ' ';
      string keyword;
      bool changePath = true;
      char mainSelect = ' ';
      Caretaker keeper = new Caretaker();

      Console.WriteLine("Choose your work type: e = editor, f = finder of words, i = indexing");
      mainSelect = Convert.ToChar(Console.ReadLine());
      if (mainSelect == 'f') {
        while (UserDoesNotWantExit == true) {
          if (changePath == true) {
            Console.WriteLine("Enter path to your directory:");
            path = Console.ReadLine();
            changePath = false; 
          }
          
          Console.WriteLine("Keyword is:");
          keyword = Console.ReadLine();

          WordFinder wordFinder = new WordFinder(keyword, path);
          wordFinder.Search();

          Console.WriteLine("Exit? y/n");
          select = Convert.ToChar(Console.ReadLine());

          if (select == 'y') {
            UserDoesNotWantExit = false;
          } else if (select == 'n') {
            Console.WriteLine("Change path?");
            select = Convert.ToChar(Console.ReadLine());

            if (select == 'y') {
              changePath = true;
            } else if (select == 'n') {
              continue;
            }
          }
        }
        
      } else if (mainSelect == 'e') {
        while (UserDoesNotWantExit == true) {

          if (changePath == true) {
            Console.WriteLine("Enter path to your file:");
            path = Console.ReadLine();

            changePath = false;
          }
          TextFile textFile = new TextFile(path);

          Console.WriteLine("Your action: r = read, w = write, c = close,");
          Console.WriteLine("b = bin.serialise, n = bin.deserialise");
          Console.WriteLine("x = xml.serialise, z = xml.deserialise, e = exit");
          Console.WriteLine("k = change path, s = save state, t = restore state");

          select = Convert.ToChar(Console.ReadLine());

          switch (select) {
            case 'e':
              Console.WriteLine("Are you sure about that? y/n");
              select = Convert.ToChar(Console.ReadLine());

              if (select == 'y') {
                Console.WriteLine("Have a nice day.");
                UserDoesNotWantExit = false;
              } else if (select == 'n') {
                Console.WriteLine("Okay.");
                break;
              } else {
                break;
              }
              break;
            case 'r':
              textFile.ShowText();
              break;
            case 'k':
              changePath = true;
              break;
            case 'w':
              Console.WriteLine("text to your file:");
              text = Console.ReadLine();
              textFile.WriteText(text);
              break;
            case 'c':
              textFile.Close();
              break;
            case 'x':
              FileStream serialize = new
              FileStream("c:\\Lab4\\1.txt",
              FileMode.OpenOrCreate, FileAccess.Write);
              textFile.XMLSerialize(serialize);
              serialize.Close();
              break;
            case 'z':
              FileStream deserialize = new
              FileStream("c:\\Lab4\\1.txt",  // does not work without hardcode (can not create file)
              FileMode.OpenOrCreate, FileAccess.Read);
              textFile.XMLDeserialize(deserialize);
              deserialize.Close();
              break;
            case 'b':
              FileStream binSerialize = new
              FileStream("c:\\Lab4\\1.txt",
              FileMode.OpenOrCreate);
              textFile.BinarySerialize(binSerialize);
              binSerialize.Close();
              break;
            case 'n':
              FileStream binDeserialize = new
              FileStream("c:\\Lab4\\1.txt",
              FileMode.OpenOrCreate);
              textFile.BinaryDeserialize(binDeserialize);
              binDeserialize.Close();
              break;
            case 's':
              keeper.SaveState(textFile);
              break;
            case 't':
              keeper.RestoreState(textFile);
              break;
            default:
              Console.WriteLine("Wrong operation type.");
              break;
          }
        }
      } else if (mainSelect == 'i') {
        string[] keywords = {"some word", "l", "f", "static", "zero" };
        int numberOfKeywords;
        while (UserDoesNotWantExit == true) {
          if (changePath == true) {
            Console.WriteLine("Enter path to your directory:");
            path = Console.ReadLine();
            changePath = false;
          }
          Console.WriteLine("How many keywords? (max 5)");
          numberOfKeywords = Convert.ToInt32(Console.ReadLine());
          for (int inputIndex = 0; inputIndex < numberOfKeywords; ++inputIndex) {
            Console.WriteLine("Keyword № " + (inputIndex + 1) + " = ");
            keywords[inputIndex] = Console.ReadLine();
          }
          for (int outputIndex = 0;outputIndex < numberOfKeywords; ++outputIndex) {
            Console.WriteLine("Keyword is: " + keywords[outputIndex]);
            WordFinder wordFinder = new WordFinder(keywords[outputIndex], path);
            wordFinder.Search();
          }
          Console.WriteLine("Exit? y/n");
          select = Convert.ToChar(Console.ReadLine());
          if (select == 'y') {
            UserDoesNotWantExit = false;
          } else if (select == 'n') {
            Console.WriteLine("Change path?");
            select = Convert.ToChar(Console.ReadLine());
            if (select == 'y') {
              changePath = true;
            } else if (select == 'n') {
              continue;
            }
          }
        }
        
      } else {
        Console.WriteLine("Wrong operation type");
      }
      
      Console.ReadKey();
    }
  }
}
