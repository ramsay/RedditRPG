using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MapEngine
{
  class Tile
  {
    // shitty random, 'tis grand!
    public static Random g_rand;

    public const int c_size = 50;

    private class Type
    {
      /// constants and global values

      // extension expected for tile images
      public const string c_extention = ".png";

      // expected root directory for all tile images
      public const string c_directory = "Content\\Tiles\\";

      // the following exists (temporarily, I hope) to make it possible to
      // access the global content factory
      private static Game s_game;

      public static void Initialize(Game game)
      {
        s_game = game;
      }

      /// per type variables and properties:

      // tile type name, e.g. grass, dirt etc.
      private string m_name;

      // list of different textures for this type.
      private List<Texture2D> m_textures;

      public string name
      {
        get { return String.Copy(m_name); }
      }

      public int variations
      {
        get { return m_textures.Count; }
      }

      // constructor that creates a tile type and loads tile images based
      //    on the type name.
      // files for any type should be in the form: "Content/Tiles/TYPE_X"
      //    where TYPE is the in-game/editor name of the tile, and X is an
      //    ascending integer starting at 0 (can have multiple digits).
      // note that putting tiles in subdirectories will work too, but
      //    the directory will become part of the name. For example:
      //    "Content/Tiles/Lordaeron/Summer/Grass_2" will be the 3rd image for
      //    the tile type, "Lordaeron/Summer/Grass".
      public Type(string type)
      {
        m_name = String.Copy(type);

        m_textures = new List<Texture2D>(1);

        // load as many textures for this type as we can by appending
        // increasing numbers to the name
        int i = 0;
        while (true)
        {
          try
          {
            // will throw if loading fails
            Texture2D loadedTexture = Texture2D.FromFile
            ( s_game.GraphicsDevice
            , c_directory + m_name + '_' + i.ToString() + c_extention
            );

            m_textures.Add(loadedTexture);
          }
          catch (Exception e)
          {
            // if we never found even one of the images, throw
            if (m_textures.Count == 0)
              throw e;

            // otherwise, we must have found all available tiles
            break;
          }

          ++i;
        }
      }

      // gets the texture for the tile of the desired varient
      public Texture2D GetVariant(int variant)
      {
        return m_textures[variant % m_textures.Count];
      }

    } // end class Type ////////////////////////////////////////////////////////

    /// constants and global values

    // storage for all tile types, and a map for indexing based on type name
    private static List<Type> s_types;
    //private static Dictionary<string, int> s_types;

    private static SpriteBatch s_artist;

    public static void Initialize(Game game, SpriteBatch artist)
    {
      g_rand = new Random(0);

      s_types = new List<Type>(); // should load from manifest

      s_artist = artist;

      Type.Initialize(game);
    }

    public static int typeCount
    {
      get { return s_types.Count; }
    }

    // load images for a single tile type
    public static void LoadType(string name)
    {
      s_types.Add(new Type(name));
    }

    // load a bank of tile types from a file
    // NOT YET IMPLEMENTED
    public static void LoadTypesFromFile(string file)
    {

    }

    /// per tile variables and properties

    private int m_type = 0;
    private int m_variant = 0;

    public int type
    {
      get { return m_type; }
      set
      {
        m_type = value;
        m_variant = g_rand.Next() % s_types[m_type].variations;
      }
    }

    public Tile()
    {
    }

    public void Draw(int x, int y, Color tint)
    {
      Texture2D texture = s_types[m_type].GetVariant(m_variant);
      Vector2 pos = new Vector2(x - (texture.Width - Tile.c_size), y - (texture.Height - Tile.c_size));
      s_artist.Draw(texture, pos, tint);
    }

    /// private helper functions

  } // end class Tile //////////////////////////////////////////////////////////
}
