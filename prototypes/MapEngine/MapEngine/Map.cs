using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MapEngine;
using GameTest;

namespace MapEngine
{
  class Map
  {
    // initializes global values and such
    // must call before using anything in Map
    public static void Initialize(Game game, SpriteBatch artist)
    {
      Tile.Initialize(game, artist);
    }

    /// per map variables and properties

    // width and height of the map
    private int m_width;
    private int m_height;

    private int m_drawWidth;
    private int m_drawHeight;

    private Vector2 m_cameraPos;

    private Tile[] m_tiles;

    public Vector2 cameraPos
    {
      get { return m_cameraPos; }
      set
      {
        // prevent setting to below the minimum position
        m_cameraPos.X = Math.Max(value.X, m_drawWidth / 2);
        m_cameraPos.Y = Math.Max(value.Y, m_drawHeight / 2);

        // prevent setting to above the maximum position
        m_cameraPos.X = Math.Min(m_cameraPos.X, m_width - m_drawWidth / 2);
        m_cameraPos.Y = Math.Min(m_cameraPos.Y, m_height - m_drawHeight / 2);
      }
    }

    public Map(int width, int height, int drawWidth, int drawHeight)
    {
      Reset(width, height);

      // + N % 2 is to prevent off by 1/2 errors when converting coordinates
      m_drawWidth = drawWidth + drawWidth % 2;
      m_drawHeight = drawHeight + drawHeight % 2;
    }

    // converts given world coordinates to screen coordinates
    public IntVector2 WorldToScreen(Vector2 world)
    {
      Vector2 offset = m_cameraPos - new Vector2(m_drawWidth, m_drawHeight) / 2.0f;

      IntVector2 ret = (IntVector2)((world - offset) * Tile.c_size);

      return ret;
    }

    // converts given screen coordinates to world coordinates
    public Vector2 ScreenToWorld(IntVector2 screen)
    {
      Vector2 ret = new Vector2((float)screen.X / Tile.c_size, (float)screen.Y / Tile.c_size);

      ret += m_cameraPos - new Vector2(m_drawWidth / 2, m_drawHeight / 2);

      return ret;
    }

    // draws the map at the given location with the given extents
    // mapPosX, mapPosY: the location on the screen to draw the map (top left)
    public void Draw(int mapDrawPosX, int mapDrawPosY)
    {
      int left = (int)m_cameraPos.X - m_drawWidth / 2;
      int top = (int)m_cameraPos.Y - m_drawHeight / 2;

      for (int y = 0; y < m_drawHeight; ++y)
      {
        for (int x = 0; x < m_drawWidth; ++x)
        {
          if (left + x < 0 || left + x >= m_width)
            continue;

          if (top + y < 0 || top + y >= m_height)
            continue;

          GetTile(left + x, top + y).Draw
          ( mapDrawPosX + (int)(((float)x - (m_cameraPos.X - Math.Truncate(m_cameraPos.X))) * Tile.c_size)
          , mapDrawPosY + (int)(((float)y - (m_cameraPos.Y - Math.Truncate(m_cameraPos.Y))) * Tile.c_size)
          , Color.White
          );
        }
      }
    }

    public bool IsWalkable(int x, int y)
    {
      if (x < 0 || x >= m_width)
        return false;
      if (y < 0 || y >= m_height)
        return false;

      return GetTile(x, y).type != 2;
    }

    public bool IsWalkable(IntVector2 pos)
    {
      return IsWalkable(pos.X, pos.Y);
    }

    /// private helper functions

    // converts an (x, y) index to [i]
    private int Index(int x, int y)
    {
      return y * m_width + x;
    }

    private Tile GetTile(int x, int y)
    {
      return m_tiles[Index(x, y)];
    }

    // creates or resets a blank map of given width and height
    private void Reset(int width, int height)
    {
      m_width = width;
      m_height = height;

      int tileCount = m_width * m_height;

      m_tiles = new Tile[tileCount];

      for (int i = 0; i < tileCount; ++i)
      {
        m_tiles[i] = new Tile();
        m_tiles[i].type = Tile.g_rand.Next() % Tile.typeCount;
      }

      m_cameraPos = new Vector2(0, 0);
    }

  } // end class Map ///////////////////////////////////////////////////////////
}
