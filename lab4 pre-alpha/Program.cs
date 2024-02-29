using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

namespace lab4_pre_alpha {
  class Memento {
    public string savedPath;
    public string isChangingPath {
      get { return savedPath; }
      set { savedPath = value; }
    }

    public string savedText;
  }
  public interface IOriginator {
    object getMemento();
    void setMemento (object memento);
    
   }
  [Serializable]
  public class TXTFile : IOriginator {
    public string path;
    public string textContent;
    public FileStream textFileStream;
    public byte[] fileData;
    public TXTFile() { }
    public TXTFile (string path) {
      this.path = path;
      textFileStream = new FileStream(path, FileMode.OpenOrCreate);
      byte[] temporary = new byte[textFileStream.Length];

      textFileStream.Read(temporary, 0, temporary.Length);
      fileData = temporary;

      textFileStream.Close();
    }
    
    private void GetText() {
      textFileStream = new FileStream(path, FileMode.OpenOrCreate);
      byte[] temporary = new byte[textFileStream.Length];

      textFileStream.Read(temporary, 0, temporary.Length);
      fileData = temporary;

      textFileStream.Close();
    }

    public void ShowText () {
      GetText();
      textContent = Encoding.Default.GetString(fileData);
      Console.WriteLine(textContent);
    }

    public void WriteText(string text) {
      System.IO.File.AppendAllText(path, text);
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
      TXTFile deserealised = (TXTFile)binFormatter.Deserialize(textFileStream);

      path = deserealised.path;
      textContent = deserealised.textContent;
      fileData = deserealised.fileData;
      
      textFileStream.Close();
    }
    public void XMLSerialize(FileStream textFileStream) {
      XmlSerializer xmlSerialize = new XmlSerializer(typeof(TXTFile)); //GetType does not work (i do not know why)
      xmlSerialize.Serialize(textFileStream, this);

      textFileStream.Flush();
      textFileStream.Close();
    }
    public void XMLDeserialize(FileStream textFileStream) {
      XmlSerializer xmlSerialize = new XmlSerializer(typeof(TXTFile)); //GetType does not work (i do not know why)
      TXTFile deserealised = (TXTFile)xmlSerialize.Deserialize(textFileStream);

      path = deserealised.path;
      textContent = deserealised.textContent;
      fileData = deserealised.fileData;

      textFileStream.Close();
    }
    object IOriginator.getMemento() {
      textFileStream = new FileStream(path, FileMode.OpenOrCreate);
      byte[] temporary = new byte[textFileStream.Length];

      textFileStream.Read(temporary, 0, temporary.Length);
      fileData = temporary;

      textFileStream.Close();
      textContent = Encoding.Default.GetString(fileData);
      Console.WriteLine("File saved");
      return new Memento() {
        savedPath = path, savedText = textContent
      };
    }
    void IOriginator.setMemento(object memento) {
      if (memento is Memento) {

        System.IO.File.Delete(path);

        var newMemento = memento as Memento;
        path = newMemento.savedPath;
        textContent = newMemento.savedText;
        FileStream replaceText = new FileStream(path, FileMode.OpenOrCreate);
        StreamWriter writer = new StreamWriter(replaceText);
        writer.Write(textContent);
        writer.Close();
        replaceText.Close();
        Console.WriteLine("File restored");
      }
    }

  }
  class WordFinder {
    private string keyword;
    private string innerPath;
    private string textContent;
    public WordFinder (string inputKeyword, string path) {
      innerPath = path;
      keyword = inputKeyword;
    }
    public void Search () {
      string[] directory = Directory.GetFiles(innerPath);
      bool coincidenceNotFound = true;
      bool noWordInFile = true;

      foreach (string filename in directory) {
        FileStream searchFileStream = new FileStream(filename, FileMode.OpenOrCreate);
        byte[] temporary = new byte[searchFileStream.Length];

        searchFileStream.Read(temporary, 0, temporary.Length);
        textContent = Encoding.Default.GetString(temporary);
        if (textContent.Contains(keyword)) {
          coincidenceNotFound = false;
          noWordInFile = false;
        }
        if (!coincidenceNotFound) {
          Console.WriteLine("Contain keyword in " + filename);
          coincidenceNotFound = true;
        }
        if (noWordInFile) {
          Console.WriteLine("File " + filename + " does not contain keyword " + keyword);
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
      string path = "my program does not work without that";
      string text;
      bool isExited = true;
      char select;
      string keyword;
      bool isChangingPath = true;
      char mainSelect;
      Caretaker skiperTheKeeper = new Caretaker();

      Console.WriteLine("Choose your work type: e = editor, f = finder of words, i = indexing");
      mainSelect = Convert.ToChar(Console.ReadLine());
      if (mainSelect == 'f') {
        while (isExited == true) {
          if (isChangingPath == true) {
            Console.WriteLine("Enter path to your directory:");
            path = Console.ReadLine();
            isChangingPath = false; 
          }
          
          Console.WriteLine("Keyword is:");
          keyword = Console.ReadLine();

          WordFinder wordFinder = new WordFinder(keyword, path);
          wordFinder.Search();

          Console.WriteLine("Exit? y/n");
          select = Convert.ToChar(Console.ReadLine());

          if (select == 'y') {
            isExited = false;
          } else if (select == 'n') {
            Console.WriteLine("Change path?");
            select = Convert.ToChar(Console.ReadLine());

            if (select == 'y') {
              isChangingPath = true;
            } else if (select == 'n') {
              continue;
            }
          }
        }
        
      } else if (mainSelect == 'e') {
        while (isExited == true) {

          if (isChangingPath == true) {
            Console.WriteLine("Enter path to your file:");
            path = Console.ReadLine();

            isChangingPath = false;
          }
          TXTFile TXTFile = new TXTFile(path);

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
                isExited = false;
              } else if (select == 'n') {
                Console.WriteLine("Okay.");
                break;
              } else {
                break;
              }
              break;
            case 'r':
              TXTFile.ShowText();
              break;
            case 'k':
              isChangingPath = true;
              break;
            case 'w':
              Console.WriteLine("text to your file:");
              text = Console.ReadLine();
              TXTFile.WriteText(text);
              break;
            case 'c':
              TXTFile.Close();
              break;
            case 'x':
              FileStream serialize = new
              FileStream("c:\\Lab4\\1.txt",
              FileMode.OpenOrCreate, FileAccess.Write);
              TXTFile.XMLSerialize(serialize);
              serialize.Close();
              break;
            case 'z':
              FileStream deserialize = new
              FileStream("c:\\Lab4\\1.txt",  // does not work without hardcode (can not create file)
              FileMode.OpenOrCreate, FileAccess.Read);
              TXTFile.XMLDeserialize(deserialize);
              deserialize.Close();
              break;
            case 'b':
              FileStream binSerialize = new
              FileStream("c:\\Lab4\\1.txt",
              FileMode.OpenOrCreate);
              TXTFile.BinarySerialize(binSerialize);
              binSerialize.Close();
              break;
            case 'n':
              FileStream binDeserialize = new
              FileStream("c:\\Lab4\\1.txt",
              FileMode.OpenOrCreate);
              TXTFile.BinaryDeserialize(binDeserialize);
              binDeserialize.Close();
              break;
            case 's':
              skiperTheKeeper.SaveState(TXTFile);
              break;
            case 't':
              skiperTheKeeper.RestoreState(TXTFile);
              break;
            default:
              Console.WriteLine("Wrong operation type.");
              break;
          }
        }
      } else if (mainSelect == 'i') {
        string[] keywords = {"some word", "l", "f", "static", "zero" };
        int numberOfKeywords;
        while (isExited == true) {
          if (isChangingPath == true) {
            Console.WriteLine("Enter path to your directory:");
            path = Console.ReadLine();
            isChangingPath = false;
          }
          Console.WriteLine("How many keyword? (max 5)");
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
            isExited = false;
          } else if (select == 'n') {
            Console.WriteLine("Change path?");
            select = Convert.ToChar(Console.ReadLine());
            if (select == 'y') {
              isChangingPath = true;
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
