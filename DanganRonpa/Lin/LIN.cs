﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace HyoutaTools.DanganRonpa.Lin {
	class ScriptEntry {
		public byte Type;
		public byte[] Arguments;

		public string Text = null;
		public string IdentifyString = null;

		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append( Type.ToString( "X2" ) ).Append( ":" );
			foreach ( byte a in Arguments ) { sb.Append( " " ).Append( a.ToString( "X2" ) ); }
			if ( Text != null ) { sb.Append( " / " ).Append( Text ); }
			return sb.ToString();
		}

		public string FormatForGraceNote() {
			if ( Type == 0x02 ) {
				if ( Text == null ) { throw new Exception( "TextType ScriptEntry is missing Text!" ); }
				return Text;
			} else {
				StringBuilder sb = new StringBuilder();
				sb.Append( '<' );

				switch ( Type ) {
					//case 0x00: entry.Arguments = new byte[2]; break;
					//case 0x01: entry.Arguments = new byte[3]; break;
					//case 0x03: entry.Arguments = new byte[1]; break;
					//case 0x04: entry.Arguments = new byte[4]; break;
					case 0x05:
						sb.Append( "FMV:" );
						foreach ( byte a in Arguments ) { sb.Append( " " ).Append( a.ToString() ); }
						break;
					//case 0x06: entry.Arguments = new byte[8]; break;
					case 0x08:
						sb.Append( "Voice: " );
						sb.Append( '[' ).Append( Util.CharacterIdToName( Arguments[0] ) ).Append( ']' );
						for ( int i = 1; i < Arguments.Length; ++i ) { sb.Append( " " ).Append( Arguments[i].ToString() ); }
						break;
					case 0x09:
						sb.Append( "Music:" );
						foreach ( byte a in Arguments ) { sb.Append( " " ).Append( a.ToString() ); }
						break;
					case 0x0A:
						sb.Append( "Sound:" );
						foreach ( byte a in Arguments ) { sb.Append( " " ).Append( a.ToString() ); }
						break;
					//case 0x0B: entry.Arguments = new byte[2]; break;
					//case 0x0C: entry.Arguments = new byte[2]; break;
					//case 0x0D: entry.Arguments = new byte[3]; break;
					//case 0x0E: entry.Arguments = new byte[2]; break;
					//case 0x0F: entry.Arguments = new byte[3]; break;
					//case 0x10: entry.Arguments = new byte[3]; break;
					//case 0x11: entry.Arguments = new byte[4]; break;
					//case 0x14: entry.Arguments = new byte[3]; break;
					//case 0x15: entry.Arguments = new byte[3]; break;
					case 0x19:
						sb.Append( "LoadScript:" );
						foreach ( byte a in Arguments ) { sb.Append( " " ).Append( a.ToString() ); }
						break;
					//case 0x1A: entry.Arguments = new byte[0]; break;
					//case 0x1B: entry.Arguments = new byte[3]; break;
					//case 0x1C: entry.Arguments = new byte[0]; break;
					case 0x1E:
						sb.Append( "Sprite: " );
						sb.Append( "0x" ).Append( Arguments[0].ToString( "X2" ) ).Append( ' ' );
						sb.Append( '[' ).Append( Util.CharacterIdToName( Arguments[1] ) ).Append( ']' );
						for ( int i = 2; i < Arguments.Length; ++i ) { sb.Append( " " ).Append( Arguments[i].ToString() ); }
						break;
					//case 0x1F: entry.Arguments = new byte[7]; break;
					//case 0x20: entry.Arguments = new byte[5]; break;
					case 0x21:
						sb.Append( "Speaker: " );
						sb.Append( '[' ).Append( Util.CharacterIdToName( Arguments[0] ) ).Append( ']' );
						for ( int i = 1; i < Arguments.Length; ++i ) { sb.Append( " " ).Append( Arguments[i].ToString() ); }
						break;
					//case 0x22: entry.Arguments = new byte[3]; break;
					//case 0x23: entry.Arguments = new byte[5]; break;
					//case 0x25: entry.Arguments = new byte[2]; break;
					//case 0x26: entry.Arguments = new byte[3]; break;
					//case 0x27: entry.Arguments = new byte[1]; break;
					//case 0x29: entry.Arguments = new byte[1]; break;
					//case 0x2A: entry.Arguments = new byte[2]; break;
					//case 0x2B: entry.Arguments = new byte[1]; break;
					//case 0x2E: entry.Arguments = new byte[2]; break;
					//case 0x30: entry.Arguments = new byte[3]; break;
					//case 0x33: entry.Arguments = new byte[4]; break;
					//case 0x34: entry.Arguments = new byte[2]; break;
					//case 0x38: entry.Arguments = new byte[5]; break;
					//case 0x39: entry.Arguments = new byte[5]; break;
					case 0x3A:
						sb.Append( "WaitPlayerInput" );
						if ( Arguments.Length != 0 ) { throw new Exception( "0x3A has arguments!" ); }
						break;
					case 0x3B:
						sb.Append( "WaitFrame" );
						if ( Arguments.Length != 0 ) { throw new Exception( "0x3B has arguments!" ); }
						break;
					//case 0x3C: entry.Arguments = new byte[0]; break;
					default:
						sb.Append( "0x" ).Append( Type.ToString( "X2" ) ).Append( ":" );
						foreach ( byte a in Arguments ) { sb.Append( " " ).Append( "0x" ).Append( a.ToString( "X2" ) ); }
						break;
				}

				sb.Append( '>' );
				return sb.ToString();
			}
		}
	}

	class LIN {
		byte[] OriginalFile;
		int Type;
		int HeaderSize;
		int Filesize;
		int TextBlockLocation;

		public List<ScriptEntry> ScriptData;
		int TextAmount;

		public int UnalignedFilesize;

		public List<KeyValuePair<int, string>> UnreferencedText;

		public LIN( String filename ) {
			if ( !LoadFile( System.IO.File.ReadAllBytes( filename ) ) ) {
				throw new Exception( "LIN: Load Failed!" );
			}
		}
		public LIN( byte[] Bytes ) {
			if ( !LoadFile( Bytes ) ) {
				throw new Exception( "LIN: Load Failed!" );
			}
		}


		private bool LoadFile( byte[] Bytes ) {
			OriginalFile = Bytes;

			Type = BitConverter.ToInt32( Bytes, 0x0 );
			HeaderSize = BitConverter.ToInt32( Bytes, 0x4 );

			if ( Type == 2 ) {
				TextBlockLocation = BitConverter.ToInt32( Bytes, 0x8 );
				Filesize = BitConverter.ToInt32( Bytes, 0xC );

				ScriptData = CreateScriptDataFromOriginal();
				TextAmount = BitConverter.ToInt32( Bytes, TextBlockLocation );
				GrabTextForScriptEntries();
				FigureOutNameDisplayForEachEntry();
			} else if ( Type == 1 ) {
				Filesize = BitConverter.ToInt32( Bytes, 0x8 );
				TextBlockLocation = Filesize;

				ScriptData = CreateScriptDataFromOriginal();
			} else {
				return false;
			}

			return true;
		}

		public void GrabTextForScriptEntries() {
			List<int> TextIds = new List<int>( TextAmount ); // yes list isn't a good type for this but I can't find a search tree in C#'s default implementation and it's not like this is performance critical

			for ( int i = 0; i < ScriptData.Count; ++i ) {
				if ( ScriptData[i].Type == 0x02 ) {
					// if it's a text entry, grab the corresponding text
					byte first = ScriptData[i].Arguments[0];
					byte second = ScriptData[i].Arguments[1];

					int TextId = ( first << 8 | second );

					if ( TextId >= TextAmount ) {
						throw new Exception( "TextId may not be larger or equal to TextAmount!" );
					}

					TextIds.Add( TextId );

					int TextLocation = BitConverter.ToInt32( OriginalFile, TextBlockLocation + ( ( TextId + 1 ) * 4 ) );
					int NextTextLocation = BitConverter.ToInt32( OriginalFile, TextBlockLocation + ( ( TextId + 2 ) * 4 ) );
					ScriptData[i].Text = Encoding.Unicode.GetString( OriginalFile, TextBlockLocation + TextLocation, NextTextLocation - TextLocation );
				} else {
					ScriptData[i].Text = null;
				}
			}

			UnreferencedText = new List<KeyValuePair<int, string>>();
			for ( int i = 0; i < TextAmount; ++i ) {
				if ( !TextIds.Contains( i ) ) {
					// Unreferenced string found, add.

					int TextLocation = BitConverter.ToInt32( OriginalFile, TextBlockLocation + ( ( i + 1 ) * 4 ) );
					int NextTextLocation = BitConverter.ToInt32( OriginalFile, TextBlockLocation + ( ( i + 2 ) * 4 ) );
					string Text = Encoding.Unicode.GetString( OriginalFile, TextBlockLocation + TextLocation, NextTextLocation - TextLocation );

					UnreferencedText.Add( new KeyValuePair<int, string>( i, Text ) );
				}
			}

		}

		public List<ScriptEntry> CreateScriptDataFromOriginal() {
			List<ScriptEntry> Script = new List<ScriptEntry>();

			for ( int i = HeaderSize; i < TextBlockLocation; i++ ) {
				if ( OriginalFile[i] == 0x70 ) {
					i++;
					ScriptEntry entry = new ScriptEntry();
					entry.Type = OriginalFile[i];
					switch ( entry.Type ) {
						case 0x00: entry.Arguments = new byte[2]; break;
						case 0x01: entry.Arguments = new byte[3]; break;
						case 0x02: entry.Arguments = new byte[2]; break;
						case 0x03: entry.Arguments = new byte[1]; break;
						case 0x04: entry.Arguments = new byte[4]; break;
						case 0x05: entry.Arguments = new byte[2]; break;
						case 0x06: entry.Arguments = new byte[8]; break;
						case 0x08: entry.Arguments = new byte[5]; break;
						case 0x09: entry.Arguments = new byte[3]; break;
						case 0x0A: entry.Arguments = new byte[3]; break;
						case 0x0B: entry.Arguments = new byte[2]; break;
						case 0x0C: entry.Arguments = new byte[2]; break;
						case 0x0D: entry.Arguments = new byte[3]; break;
						case 0x0E: entry.Arguments = new byte[2]; break;
						case 0x0F: entry.Arguments = new byte[3]; break;
						case 0x10: entry.Arguments = new byte[3]; break;
						case 0x11: entry.Arguments = new byte[4]; break;
						case 0x14: entry.Arguments = new byte[3]; break;
						case 0x15: entry.Arguments = new byte[3]; break;
						case 0x19: entry.Arguments = new byte[3]; break;
						case 0x1A: entry.Arguments = new byte[0]; break;
						case 0x1B: entry.Arguments = new byte[3]; break;
						case 0x1C: entry.Arguments = new byte[0]; break;
						case 0x1E: entry.Arguments = new byte[5]; break;
						case 0x1F: entry.Arguments = new byte[7]; break;
						case 0x20: entry.Arguments = new byte[5]; break;
						case 0x21: entry.Arguments = new byte[1]; break;
						case 0x22: entry.Arguments = new byte[3]; break;
						case 0x23: entry.Arguments = new byte[5]; break;
						case 0x25: entry.Arguments = new byte[2]; break;
						case 0x26: entry.Arguments = new byte[3]; break;
						case 0x27: entry.Arguments = new byte[1]; break;
						case 0x29: entry.Arguments = new byte[1]; break;
						case 0x2A: entry.Arguments = new byte[2]; break;
						case 0x2B: entry.Arguments = new byte[1]; break;
						case 0x2E: entry.Arguments = new byte[2]; break;
						case 0x30: entry.Arguments = new byte[3]; break;
						case 0x33: entry.Arguments = new byte[4]; break;
						case 0x34: entry.Arguments = new byte[2]; break;
						case 0x38: entry.Arguments = new byte[5]; break;
						case 0x39: entry.Arguments = new byte[5]; break;
						case 0x3A: entry.Arguments = new byte[0]; break;
						case 0x3B: entry.Arguments = new byte[0]; break;
						case 0x3C: entry.Arguments = new byte[0]; break;
						default:
							List<byte> VariableLengthArgs = new List<byte>();
							while ( OriginalFile[i + 1] != 0x70 ) { VariableLengthArgs.Add( OriginalFile[i + 1] ); ++i; }
							entry.Arguments = VariableLengthArgs.ToArray();
							Script.Add( entry );
							continue;
					}

					for ( int j = 0; j < entry.Arguments.Length; ++j ) {
						entry.Arguments[j] = OriginalFile[i + 1];
						++i;
					}
					Script.Add( entry );

				} else {
					// reached end of file?
					for ( ; i < TextBlockLocation; i++ ) {
						if ( OriginalFile[i] != 0x00 ) {
							throw new Exception( "script entry doesn't start with 0x70, abort" );
						}
					}
					return Script;
				}
			}
			return Script;
		}

		public void FigureOutNameDisplayForEachEntry() {
			String CurrentName = "";

			foreach ( ScriptEntry s in ScriptData ) {
				switch ( s.Type ) {
					case 0x21:
						CurrentName = Util.CharacterIdToName( s.Arguments[0] );
						break;
					case 0x1E:
						CurrentName = Util.CharacterIdToName( s.Arguments[1] );
						break;
					case 0x02:
						s.IdentifyString = CurrentName;
						break;
					default:
						break;
				}
			}
		}


		public byte[] CreateFile( int Alignment ) {
			List<byte> file = new List<byte>();

			// header
			file.AddRange( BitConverter.GetBytes( Type ) );
			file.AddRange( BitConverter.GetBytes( HeaderSize ) );

			if ( Type == 2 ) {
				file.AddRange( BitConverter.GetBytes( TextBlockLocation ) );
				file.AddRange( BitConverter.GetBytes( Filesize ) );
			} else if ( Type == 1 ) {
				file.AddRange( BitConverter.GetBytes( Filesize ) );
			}

			Dictionary<int, string> TextList = new Dictionary<int, string>();
			if ( Type == 2 ) {
				// find all text
				// first add the unref'd strings
				foreach ( KeyValuePair<int, string> k in UnreferencedText ) {
					TextList.Add( k.Key, k.Value );
				}

				// then find all text from the script
				TextAmount = 0;
				foreach ( ScriptEntry s in ScriptData ) {
					if ( s.Type == 0x02 ) {
						while ( TextList.ContainsKey( TextAmount ) ) { ++TextAmount; }
						TextList.Add( TextAmount, s.Text );

						s.Arguments[0] = (byte)( TextAmount >> 8 & 0xFF );
						s.Arguments[1] = (byte)( TextAmount & 0xFF );

						++TextAmount;
					}
				}
				TextAmount = Math.Max( TextAmount, TextList.Keys.Max() + 1 );
				// This should add all Text and set the TextAmount properly, I think?
			}

			// add script
			foreach ( ScriptEntry s in ScriptData ) {
				file.Add( 0x70 );
				file.Add( s.Type );
				file.AddRange( s.Arguments );
			}

			while ( file.Count % 4 != 0 ) {
				file.Add( 0x00 );
			}

			TextBlockLocation = file.Count;
			byte[] tblbytes = BitConverter.GetBytes( TextBlockLocation );
			file[0x08] = tblbytes[0];
			file[0x09] = tblbytes[1];
			file[0x0A] = tblbytes[2];
			file[0x0B] = tblbytes[3];

			if ( Type == 2 ) {
				AddText( file, TextList );
				while ( file.Count % 4 != 0 ) {
					file.Add( 0x00 );
				}
				Filesize = file.Count;
				byte[] fsbytes = BitConverter.GetBytes( Filesize );
				file[0x0C] = fsbytes[0];
				file[0x0D] = fsbytes[1];
				file[0x0E] = fsbytes[2];
				file[0x0F] = fsbytes[3];
			} else if ( Type == 1 ) {
				Filesize = TextBlockLocation;
			}

			while ( file.Count % Alignment != 0 ) {
				file.Add( 0x00 );
			}

			return file.ToArray();
		}

		private void AddText( List<byte> file, Dictionary<int, string> TextList ) {


			file.AddRange( BitConverter.GetBytes( TextAmount ) );

			int[] startpoints = new int[TextAmount];
			int total = 8 + TextAmount * 4;
			for ( int i = 0; i < TextAmount; ++i ) {
				String Text;
				if ( TextList.ContainsKey( i ) ) {
					Text = TextList[i];
				} else {
					Text = "";
				}

				if ( !Text.EndsWith( "\0" ) ) {
					Text = Text + '\0';
				}

				Byte[] bytetext = Encoding.Unicode.GetBytes( Text );
				if ( bytetext[0] != 0xFF || bytetext[1] != 0xFE ) {
					Byte[] tmp = new byte[bytetext.Length + 2];
					tmp[0] = 0xFF;
					tmp[1] = 0xFE;

					bytetext.CopyTo( tmp, 2 );

					bytetext = tmp;
				}
				startpoints[i] = total;
				total += bytetext.Length;
			}

			UnalignedFilesize = TextBlockLocation + total;

			foreach ( int s in startpoints ) {
				file.AddRange( BitConverter.GetBytes( s ) );
			}
			file.AddRange( BitConverter.GetBytes( total ) );

			for ( int i = 0; i < TextAmount; ++i ) {
				String Text;
				if ( TextList.ContainsKey( i ) ) {
					Text = TextList[i];
				} else {
					Text = "";
				}

				if ( !Text.EndsWith( "\0" ) ) {
					Text = Text + '\0';
				}

				Byte[] bytetext = Encoding.Unicode.GetBytes( Text );
				if ( bytetext[0] != 0xFF || bytetext[1] != 0xFE ) {
					Byte[] tmp = new byte[bytetext.Length + 2];
					tmp[0] = 0xFF;
					tmp[1] = 0xFE;

					bytetext.CopyTo( tmp, 2 );

					bytetext = tmp;
				}
				file.AddRange( bytetext );
			}

		}






		public void GetSQL( String ConnectionString ) {
			SQLiteConnection Connection = new SQLiteConnection( ConnectionString );
			Connection.Open();

			ScriptData.Clear();
			TextAmount = 0;
			UnreferencedText = new List<KeyValuePair<int, string>>();

			using ( SQLiteTransaction Transaction = Connection.BeginTransaction() )
			using ( SQLiteCommand Command = new SQLiteCommand( Connection ) ) {
				Command.CommandText = "SELECT english, PointerRef, IdentifyPointerRef FROM Text ORDER BY id";
				SQLiteDataReader r = Command.ExecuteReader();
				while ( r.Read() ) {
					String SQLText;

					try {
						SQLText = r.GetString( 0 ).Replace( "''", "'" );
					} catch ( System.InvalidCastException ) {
						SQLText = "";
					}

					int PointerRef = r.GetInt32( 1 );
					int IdentPtrRef = r.GetInt32( 2 );


					if ( PointerRef == 1 ) {
						// game code
						while ( true ) {
							int commandstart = SQLText.IndexOf( '<' );
							int commandend = SQLText.IndexOf( '>' );

							if ( commandstart == -1 ) {
								break;
							}

							string cmd = SQLText.Substring( commandstart + 1, ( commandend - commandstart ) - 1 );
							SQLText = SQLText.Substring( commandend + 1 );

							ScriptEntry e = new ScriptEntry();

							if ( cmd.Contains( ':' ) ) {
								// command with args
								string[] bytestrs = cmd.Split( ' ' );
								string typestr = bytestrs[0].Replace( ":", "" );
								switch ( typestr ) {
									case "FMV": e.Type = 0x05; break;
									case "Voice": e.Type = 0x08; break;
									case "Music": e.Type = 0x09; break;
									case "Sound": e.Type = 0x0A; break;
									case "LoadScript": e.Type = 0x19; break;
									case "Sprite": e.Type = 0x1E; break;
									case "Speaker": e.Type = 0x21; break;
									default: e.Type = Util.ParseDecOrHex( typestr ); break;
								}

								List<byte> args = new List<byte>();
								for ( int i = 1; i < bytestrs.Length; ++i ) {
									if ( bytestrs[i].Contains( '[' ) && bytestrs[i].Contains( ']' ) ) {
										args.Add( Util.NameToCharacterId( bytestrs[i].Replace( "[", "" ).Replace( "]", "" ) ) );
									} else {
										args.Add( Util.ParseDecOrHex( bytestrs[i] ) );
									}
								}
								e.Arguments = args.ToArray();

							} else {
								// command without args
								if ( cmd == "WaitPlayerInput" ) {
									e.Type = 0x3A;
								} else if ( cmd == "WaitFrame" ) {
									e.Type = 0x3B;
								} else {
									e.Type = Util.ParseDecOrHex( cmd );
								}
								e.Arguments = new byte[0];
							}

							ScriptData.Add( e );

						}

					} else if ( PointerRef == 2 ) {
						// normal text
						if ( !SQLText.StartsWith( "<REMOVE>" ) ) {
							bool first = true;
							foreach ( string s in SQLText.Split( '\f' ) ) {
								if ( !first ) {
									ScriptEntry wait = new ScriptEntry();
									wait.Type = 0x3A;
									wait.Arguments = new byte[0];
									ScriptData.Add( wait );
								}

								ScriptEntry e = new ScriptEntry();
								e.Type = 0x02;
								e.Arguments = new byte[2];
								e.Arguments[0] = 0xFF; // will be fixed later in CreateFile()
								e.Arguments[1] = 0xFF;
								e.Text = s;
								ScriptData.Add( e );
								first = false;
							}
						}

					} else if ( PointerRef == 3 ) {
						// unreferenced text
						UnreferencedText.Add( new KeyValuePair<int, string>( IdentPtrRef, SQLText ) );

					} else {
						throw new Exception( "Unrecognized PointerRef type!" );
					}


				}

				Transaction.Rollback();
			}
			return;
		}



	}
}