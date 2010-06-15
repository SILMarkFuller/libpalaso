using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Palaso.WritingSystems;

namespace Palaso.WritingSystems
{
	public class WritingSystemStoreBase : IWritingSystemStore
	{
		private Dictionary<string, WritingSystemDefinition> _writingSystems;
		private Dictionary<string, DateTime> _writingSystemsToIgnore;

		/// <summary>
		/// Use the default repository
		/// </summary>
		public WritingSystemStoreBase()
		{
			_writingSystems = new Dictionary<string, WritingSystemDefinition>();
			_writingSystemsToIgnore = new Dictionary<string, DateTime>();
			//_sharedStore = LdmlSharedWritingSystemCollection.Singleton;
		}

		public IEnumerable<WritingSystemDefinition> WritingSystemDefinitions
		{
			get
			{
				return _writingSystems.Values;
			}
		}

		protected IDictionary<string, DateTime> WritingSystemsToIgnore
		{
			get
			{
				return _writingSystemsToIgnore;
			}
		}

		virtual public WritingSystemDefinition CreateNew()
		{
			WritingSystemDefinition retval = new WritingSystemDefinition();

			//!!! TODO: Add to shared

			return retval;
		}

		virtual protected LdmlAdaptor CreateLdmlAdaptor()
		{
			return new LdmlAdaptor();
		}

		virtual public void Remove(string identifier)
		{
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (!_writingSystems.ContainsKey(identifier))
			{
				throw new ArgumentOutOfRangeException("identifier");
			}
			// Delete from us
			//??? Do we really delete or just mark for deletion?
			_writingSystems.Remove(identifier);
			_writingSystemsToIgnore.Remove(identifier);
			//TODO: Could call the shared store to advise that one has been removed.
			//TODO: This may be useful if writing systems were reference counted.
		}

		virtual public void LastChecked(string identifier, DateTime dateModified)
		{
			if (_writingSystemsToIgnore.ContainsKey(identifier))
			{
				_writingSystemsToIgnore[identifier] = dateModified;
			}
			else
			{
				_writingSystemsToIgnore.Add(identifier, dateModified);
			}
		}

		protected void Clear()
		{
			_writingSystems.Clear();
		}

		public WritingSystemDefinition MakeDuplicate(WritingSystemDefinition definition)
		{
			if (definition == null)
			{
				throw new ArgumentNullException("definition");
			}
			return definition.Clone();
		}


		public bool Exists(string identifier)
		{
			return _writingSystems.ContainsKey(identifier);
		}

		public virtual void Set(WritingSystemDefinition ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			string newID = (!String.IsNullOrEmpty(ws.RFC5646)) ? ws.RFC5646 : "unknown";
			if (_writingSystems.ContainsKey(newID) && newID != ws.StoreID)
			{
				throw new ArgumentException(String.Format("Unable to store writing system '{0:s}' because this id already exists.  Please change this writing system before storing.", newID));
			}
			//??? How do we update
			//??? Is it sufficient to just set it, or can we not change the reference in case someone else has it too
			//??? i.e. Do we need a ws.Copy(WritingSystemDefinition)?
			if (!String.IsNullOrEmpty(ws.StoreID) && _writingSystems.ContainsKey(ws.StoreID))
			{
				_writingSystems.Remove(ws.StoreID);
			}
			ws.StoreID = newID;
			_writingSystems[ws.StoreID] = ws;
		}

		public string GetNewStoreIDWhenSet(WritingSystemDefinition ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException("ws");
			}
			return (!String.IsNullOrEmpty(ws.RFC5646)) ? ws.RFC5646 : "unknown";
		}

		public bool CanSet(WritingSystemDefinition ws)
		{
			if (ws == null)
			{
				return false;
			}
			string newID = (!String.IsNullOrEmpty(ws.RFC5646)) ? ws.RFC5646 : "unknown";
			return newID == ws.StoreID || !_writingSystems.ContainsKey(newID);
		}

		public WritingSystemDefinition Get(string identifier)
		{
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (!_writingSystems.ContainsKey(identifier))
			{
				throw new ArgumentOutOfRangeException("identifier");
			}
			return _writingSystems[identifier];
		}

		public int Count
		{
			get
			{
				return _writingSystems.Count;
			}
		}

		virtual public void Save()
		{
		}

		virtual protected void OnChangeNotifySharedStore(WritingSystemDefinition ws)
		{
			DateTime lastDateModified;
			if (_writingSystemsToIgnore.TryGetValue(ws.Id, out lastDateModified) && ws.DateModified > lastDateModified)
				_writingSystemsToIgnore.Remove(ws.Id);
		}

		virtual protected void OnRemoveNotifySharedStore()
		{
		}

		virtual public IEnumerable<WritingSystemDefinition> WritingSystemsNewerIn(IEnumerable<WritingSystemDefinition> rhs)
		{
			if (rhs == null)
			{
				throw new ArgumentNullException("rhs");
			}
			List<WritingSystemDefinition> newerWritingSystems = new List<WritingSystemDefinition>();
			foreach (WritingSystemDefinition ws in rhs)
			{
				if (ws == null)
				{
					throw new ArgumentNullException("rhs", "rhs contains a null WritingSystemDefinition");
				}
				if (_writingSystems.ContainsKey(ws.RFC5646))
				{
					DateTime lastDateModified;
					if ((!_writingSystemsToIgnore.TryGetValue(ws.RFC5646, out lastDateModified) || ws.DateModified > lastDateModified)
						&& (ws.DateModified > _writingSystems[ws.RFC5646].DateModified))
					{
						newerWritingSystems.Add(ws.Clone());
					}
				}
			}
			return newerWritingSystems;
		}

	}
}
