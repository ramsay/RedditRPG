using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using MapEngine;

namespace GameTest
{
  public class IntVector2
  {
    public int X;
    public int Y;

    public IntVector2(int x, int y)
    {
      X = x;
      Y = y;
    }

    public static IntVector2 operator /(IntVector2 lhs, int rhs)
    {
      return new IntVector2(lhs.X / rhs, lhs.Y / rhs);
    }

    public static IntVector2 operator *(IntVector2 lhs, int rhs)
    {
      return new IntVector2(lhs.X * rhs, lhs.Y * rhs);
    }

    public static IntVector2 operator + (IntVector2 lhs, IntVector2 rhs)
    {
      return new IntVector2(lhs.X + rhs.X, lhs.Y + rhs.Y);
    }

    public static explicit operator Vector2(IntVector2 rhs)
    {
      return new Vector2(rhs.X, rhs.Y);
    }

    public static explicit operator IntVector2(Vector2 rhs)
    {
      return new IntVector2((int)rhs.X, (int)rhs.Y);
    }
  }

  class GameTest : Microsoft.Xna.Framework.Game
  {
    GraphicsDeviceManager m_graphicsManager;
    SpriteBatch m_spriteBatch;

    KeyboardState m_keyboardState;
    MouseState  m_mouseState;

    IntVector2  m_screenSize;
    IntVector2  m_worldSize;

    IntVector2  m_player;
    bool        m_idle = true;
    IntVector2  m_direction;

    int         m_moveFactor = 150;
    int         m_linearSpeed = 15;
    int         m_diagonalSpeed = 10;

    Texture2D   m_playerTexture;

    Map m_map;

    static void Main()
    {
      new GameTest().Run();
    }

    public GameTest()
    {
      m_graphicsManager = new GraphicsDeviceManager(this);

      m_screenSize = new IntVector2(750, 550);
      m_worldSize = m_screenSize / 50;

      m_player = new IntVector2(0, 0);

      m_graphicsManager.PreferredBackBufferWidth = m_screenSize.X;
      m_graphicsManager.PreferredBackBufferHeight = m_screenSize.Y;

      Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
      m_keyboardState = Keyboard.GetState();

      m_spriteBatch = new SpriteBatch(GraphicsDevice);

      Map.Initialize(this, m_spriteBatch);

      base.Initialize();
    }

    protected override void LoadContent()
    {
      Tile.LoadType("derp");
      Tile.LoadType("herp");
      Tile.LoadType("nope");

      m_map = new Map(100, 100, m_screenSize.X / Tile.c_size + 2, m_screenSize.Y / Tile.c_size + 2);

      try
      {
        /*
        m_texture = Texture2D.FromFile(GraphicsDevice, "Content/player.png"); /*/
        m_playerTexture = Content.Load<Texture2D>("player.png");//*/
      }
      catch (Exception)
      {
        Console.WriteLine("Failed to load texture from file");
        m_playerTexture = new Texture2D(GraphicsDevice, 1, 1);
      }
    }

    protected override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      UpdateInput();

      if (m_idle)
      {
        IntVector2 movement = new IntVector2(0, 0);

        if (m_keyboardState.IsKeyDown(Keys.W))
          movement.Y -= 1;
        if (m_keyboardState.IsKeyDown(Keys.A))
          movement.X -= 1;
        if (m_keyboardState.IsKeyDown(Keys.S))
          movement.Y += 1;
        if (m_keyboardState.IsKeyDown(Keys.D))
          movement.X += 1;

        if (!(movement.X == 0 && movement.Y == 0))
        {
          // walking in a straight line is easy...
          if (movement.X == 0 || movement.Y == 0)
          {
            if (m_map.IsWalkable(m_player / m_moveFactor + movement))
            {
              m_direction = movement * m_linearSpeed;
              m_idle = false;
            }
          }
          // moving diagonally
          else
          {
            // if we can reach the desired tile AND pass through at least one corner
            if (m_map.IsWalkable(m_player / m_moveFactor + movement))
            {
              if (m_map.IsWalkable(m_player.X / m_moveFactor + movement.X, m_player.Y / m_moveFactor)
              || m_map.IsWalkable(m_player.X / m_moveFactor, m_player.Y / m_moveFactor + movement.Y)
              ) {
                m_direction = movement * m_diagonalSpeed;
                m_idle = false;
              }
            }
            // if we can't reach the diagonal tile, walk along the wall if possible
            else
            {
              if (m_map.IsWalkable(m_player.X / m_moveFactor + movement.X, m_player.Y / m_moveFactor))
              {
                movement.Y = 0;
                m_direction = movement * m_linearSpeed;
                m_idle = false;
              }
              else if (m_map.IsWalkable(m_player.X / m_moveFactor, m_player.Y / m_moveFactor + movement.Y))
              {
                movement.X = 0;
                m_direction = movement * m_linearSpeed;
                m_idle = false;
              }
            }
          }
        }
      }

      if (!m_idle)
      {
        m_player = m_player + m_direction;

        if (m_player.X % m_moveFactor == 0 && m_player.Y % m_moveFactor == 0)
        {
          m_idle = true;
        }
      }
    }

    protected override bool BeginDraw()
    {
      m_graphicsManager.GraphicsDevice.Clear(Color.CornflowerBlue);

      m_spriteBatch.Begin();

      return true;
    }

    protected override void Draw(GameTime gameTime)
    {
      m_map.cameraPos = (Vector2)m_player / m_moveFactor + new Vector2(2, 2);// +new Vector2(0.5f, 0.5f);

      m_map.Draw(0, 0);

      m_spriteBatch.Draw(m_playerTexture, (Vector2)m_map.WorldToScreen((Vector2)m_player / m_moveFactor), Color.White);

      m_spriteBatch.End();

      base.Draw(gameTime);
    }

    protected override void EndDraw()
    {
    }

    private void UpdateInput()
    {
      KeyboardState newKeyboardState = Keyboard.GetState();
      MouseState newMouseState = Mouse.GetState();

      if (newMouseState.X != m_mouseState.X || newMouseState.Y != m_mouseState.Y)
      {
        Vector2 worldPos = m_map.ScreenToWorld(new IntVector2(newMouseState.X, newMouseState.Y));
        Console.WriteLine("X: {0:D}, Y: {1:D} | WX: {2:F}, WY: {3:F}\n", newMouseState.X, newMouseState.Y, worldPos.X, worldPos.Y);
      }

      // Is the SPACE key down?
      if (newKeyboardState.IsKeyDown(Keys.Space))
      {
        // If not down last update, key has just been pressed.
        if (!m_keyboardState.IsKeyDown(Keys.Space))
        {
          // key initally down, check for triggers
        }
      }

      else if (m_keyboardState.IsKeyDown(Keys.Space))
      {
        // Key was down last update, but not down now, so
        // it has just been released.
      }

      // Update saved states
      m_keyboardState = newKeyboardState;
      m_mouseState = newMouseState;
    }

    protected override void UnloadContent()
    {
    }
  }
}
