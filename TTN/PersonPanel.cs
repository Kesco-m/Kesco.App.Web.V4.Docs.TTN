using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.DBSelect.V4;
using Kesco.Lib.Entities.Persons;
using Kesco.Lib.Entities.Stores;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Item = Kesco.Lib.Entities.Item;

namespace Kesco.App.Web.Docs.TTN
{
	//Класс объекта-контроллера определяющего поведение панели объединяющей взаимосвязанные элементы пользователького интерфейса
	//Панель используется для отображения и редактирования свойств лиц: поставщик, плательщик, грузоотправитель, грузополучатель
	public class PersonPanel
	{
		/// <summary>
		/// Тип делегата, используемый, для добавлении дополнительной информации в заголовок панели
		/// </summary>
		/// <param name="sb">Объект построитель строки, в который следует добавлять дополнительную информацию</param>
		public delegate string PostGetTitleDelegate(StringBuilder sb);

		//Ссылка на объект-страницу содержащую данную панель
		private Nakladnaya _page;
		//Строка префих используемая в идентификаторе группирующего элемента в HTML разметке данной панели
		private string _prefix;
		//Элемент управления в который помещается сводная информация о лице и связанный с соответсвующим полем документа
		private TextBox _info;
		//Элемент управления в который помещается ОКПО лица и связанный с соответсвующим полем документа
		private TextBox _code;
		//Элемент управления лицо связанный с соответсвующим полем документа
		private DBSPerson _person;
		//Элемент управления адрес лица связанный с соответсвующим полем документа
		private DBSPersonContact _address;
		//Элемент управления транспортный узел лица связанный с соответсвующим полем документа
		private DBSTransportNode _transport_node;
		//Элемент управления расчетный счет лица связанный с соответсвующим полем документа
		private DBSStore _store;
		//Элемент управления в который помещается сводная информация о расчетном счете лица и связанный с соответсвующим полем документа
		private TextBox _store_info;
		//Элемент управления отметки лица связанный с соответсвующим полем документа
		private TextBox _notes;

		//Объект склад идентификатор, которого установлен в поле Расчетный счет
		private Store _current_store = null;

		//Код ОКПО соответствующий лицу, которое установлено в поле ОКПО
		private string _current_code = null;

		// Делегат, используемый, для добавлении дополнительной информации в заголовок панели
		public PostGetTitleDelegate PostGetTitle;

		//Сссылка на связанную панель
		private PersonPanel _p;

	    private Nakladnaya n;

		/// <summary>
		/// Статический метод создания объекта-контроллера, для инициализации элементов панели и управления панелью
		/// </summary>
		/// <param name="page">Значение соответствующего поля объекта</param>
		/// <param name="prefix">Значение соответствующего поля объекта</param>
		/// <param name="info">Значение соответствующего поля объекта</param>
		/// <param name="code">Значение соответствующего поля объекта</param>
		/// <param name="person">Значение соответствующего поля объекта</param>
		/// <param name="address">Значение соответствующего поля объекта</param>
		/// <param name="transport_node">Значение соответствующего поля объекта</param>
		/// <param name="store">Значение соответствующего поля объекта</param>
		/// <param name="store_info">Значение соответствующего поля объекта</param>
		/// <param name="notes">Значение соответствующего поля объекта</param>
		/// <returns>Созданный и инициализированный объект-контроллер</returns>
		public static PersonPanel Init(Nakladnaya page, string prefix, TextBox info, TextBox code, DBSPerson person, DBSPersonContact address, DBSTransportNode transport_node, DBSStore store, TextBox store_info, TextBox notes)
		{
			PersonPanel p = new PersonPanel(page, prefix, info, code, person, address, transport_node, store, store_info, notes);
			p.Load();
			return p;
		}

		//Закрытый конструктор по умолчанию
		private PersonPanel()
		{
        }

		//Закрытый конструктор с инициализаторами закрытых полей
		private PersonPanel(Nakladnaya page, string prefix, TextBox info, TextBox code, DBSPerson person, DBSPersonContact address, DBSTransportNode transport_node, DBSStore store, TextBox store_info, TextBox notes)
		{
            n = new Nakladnaya();

			_page = page;
			_prefix = prefix;
			_info = info;
			_code = code;
			_person = person;
			_address = address;
			_transport_node = transport_node;
			_store = store;
			_store_info = store_info;
			_notes = notes;
		}

		/// <summary>
		/// Загрузка данных панели, которая вызывается после загрузки страницы
		/// </summary>
		private void Load()
		{
			_person.Changed += Person_Changed;

			_person.BeforeSearch += new BeforeSearchEventHandler((object s) => _page.Person_BeforeSearch(_person));

			_address.Changed += Person_Changed;

			if (null != _transport_node)
				_transport_node.Changed += Person_Changed;

			_store.Changed += Store_Changed;

			_store.BeforeSearch += new BeforeSearchEventHandler((object s) => _page.Store_BeforeSearch(_store, _person.Value));
			_address.BeforeSearch += new BeforeSearchEventHandler((object s) => _page.Address_BeforeSearch(_address, _person.Value));

			_person.OnRenderNtf += new RenderNtfDelegate((object s, Ntf ntf) => _page.Person_OnRenderNtf(ntf, _person));
			_address.OnRenderNtf += new RenderNtfDelegate((object s, Ntf ntf) => _page.Address_OnRenderNtf(ntf, _person, _address, _prefix));
			_store.OnRenderNtf += new RenderNtfDelegate((object s, Ntf ntf) => _page.Store_OnRenderNtf(ntf, _person, _current_store, _prefix));

			_address.GetFilter().ContactTypeId.Value = "1";
			_store.GetFilter().StoreTypeId.Value = "1,2,3";

			_info.OnRenderNtf += Info_OnRenderNtf;
			_store_info.OnRenderNtf += StoreInfo_OnRenderNtf;

			_info.RenderNtf();
			_store_info.RenderNtf();

			if (null != _code)
			{
				_code.OnRenderNtf += CodeInfo_OnRenderNtf;
				_code.RenderNtf();
			}

			Person_Changed(null, null);
			Store_Changed(null, null);
		}

		/// <summary>
		/// Метод для установки лица
		/// </summary>
		/// <param name="id">Код лица</param>
		public void SetPerson(int id)
		{
			V4Control[] ctrls = { _info, _code, _person, _address, _transport_node, _store, _store_info, _notes };
			foreach (V4Control ctrl in ctrls)
			{
				if (null == ctrl) continue;
				//ctrl.Value = null;
				ctrl.BindDocField.Value = null;
			}

			_current_store = null;
			_current_code = null;

			if (0 == id)
			{
				ClientScripts.SendSetInnerHtml(_page, _prefix + Nakladnaya.suffixTitle, GetTitle());
				return;
			}

			_person.BindDocField.Value = id.ToString();
			//_person.Value = id.ToString();

			Person_Changed(null, null);
			//Store_Changed(null, null);

			ClientScripts.SendSetInnerHtml(_page, _prefix + Nakladnaya.suffixTitle, GetTitle());
		}

		/// <summary>
		/// Метод связывает значения полей в двух панелях (Грузоотправитель-Поставщик или Грузополучатель-Плательщик)
		/// </summary>
		/// <param name="p">Ссылка на связываемую панель</param>
		public void BindTo(PersonPanel p)
		{
			PersonPanel prev = _p ?? p;

			_p = p;

			if (null != prev)
			{
				prev._person.IsDisabled = null != p;
				prev._address.IsDisabled = null != p;
				prev._store.IsDisabled = null != p;
			}

			UpdateBindedPanel(null, null);
		}

		/// <summary>
		/// Метод для обновления значений в связанной панели
		/// </summary>
		/// <param name="sender">Объект источник события в результате которого потребовалось произвести обновление</param>
		/// <param name="args">Параметры события</param>
		private void UpdateBindedPanel(object sender, ProperyChangedEventArgs args)
		{
			if (null == _p) return;

			_p._person.BindDocField.Value = _person.Value;
			_p._address.BindDocField.Value = _address.Value;
			_p._store.BindDocField.Value = _store.Value;

			_p.Person_Changed(sender, args);
			_p.Store_Changed(null, null);

			ClientScripts.SendSetInnerHtml(_page, _p._prefix + Nakladnaya.suffixTitle, _p.GetTitle());
		}

		/// <summary>
		/// Метод формирует суммарное текстовое описание из значений переданных элементов пользовательского интерфейса
		/// </summary>
		/// <param name="ctrls">Элементы из значений которых формируется описание</param>
		/// <param name="sep">Разделитель частей описания</param>
		/// <returns>Строку описание</returns>
		private string GetHtmlPersonDescription(IEnumerable<V4Control> ctrls, string sep = "; ")
		{
			StringBuilder sbDescription = new StringBuilder();

			foreach (V4Control ctrl in ctrls)
			{
				if (null == ctrl) continue;

				string textOfControl = Nakladnaya.GetCtrlText(ctrl, false);

				if (textOfControl.Length < 1) continue;

				if (sbDescription.Length > 0) sbDescription.Append(sep);

				sbDescription.Append(textOfControl);
			}

			return sbDescription.ToString();
		}

		/// <summary>
		/// Метод формирует заголовок панели
		/// </summary>
		/// <returns>Строку - заголовок панели</returns>
		public string GetTitle()
		{
			V4Control[] ctrls = { _person, _transport_node, _address, _store, _notes };

            string dataInfo = n.GetSectionHtmlDescription(ctrls);

			string strNtfInfo = string.Empty;

			using (StringWriter ntfText = new StringWriter())
			{
				//_info.RenderNtf();
				_info.RenderNtf(ntfText);

				//_code.RenderNtf();
				_code.RenderNtf(ntfText);

				foreach (V4Control ctrl in ctrls)
				{
					if (null == ctrl) continue;

					//ctrl.RenderNtf();
					ctrl.RenderNtf(ntfText);
				}

				//_store_info.RenderNtf();
				_store_info.RenderNtf(ntfText);

				if (dataInfo.Length > 0 && ntfText.GetStringBuilder().Length > 0)
					strNtfInfo = ntfText.ToString();

				dataInfo += strNtfInfo;
			}

			if (null != PostGetTitle)
			{
				return PostGetTitle(new StringBuilder(dataInfo));
			}

			return dataInfo;
		}

		/// <summary>
		/// Метод обновляет поля, значения которых зависят от установленного лица
		/// </summary>
		void UpdatePersonDeps()
		{
			if (string.IsNullOrWhiteSpace(_person.Value)) return;

			//Если установленный в элементе выбора адреса фильтр не соответсвует новому значению лица, то устанавливаем пустое значение
			//if ((ctrls[2] as DBSPersonContact).GetFilter().PersonId.Value != ctrls[0].Value)
			//	ctrls[2].Value = null;
			_address.RenderNtf();

			//Если установленный в элементе выбора счета фильтр не соответсвует новому значению лица, то устанавливаем пустое значение
			//if ((ctrls[3] as DBSStore).GetFilter().ManagerId.Value != ctrls[0].Value)
			//{
			//ctrls[3].Value = null;
			//}

			_store.RenderNtf();
		}

		/// <summary>
		/// Обработчик события изменения даты документа
		/// </summary>
		public void OnDocDateChanged()
		{
			_person.RenderNtf();
			_address.RenderNtf();
			_store.RenderNtf();

			ClientScripts.SendSetInnerHtml(_page, _prefix + Nakladnaya.suffixTitle, GetTitle());
		}

		/// <summary>
		/// Обработчик события изменения валюты документа
		/// </summary>
		public void OnCurrencyChanged()
		{
			_store.RenderNtf();
			ClientScripts.SendSetInnerHtml(_page, _prefix + Nakladnaya.suffixTitle, GetTitle());
		}

		/// <summary>
		/// Обработчик события изменения лица
		/// </summary>
		/// <param name="sender">Объект источник события</param>
		/// <param name="args">Параметры события</param>
		public void Person_Changed(object sender, ProperyChangedEventArgs args)
		{
			UpdatePersonDeps();

			if (string.IsNullOrEmpty(_person.Value))
			{
				_page.Hide(_prefix + "InfoPanel");
				_page.Hide(_prefix + "CodePanel");

				_info.Visible = false;
				_code.Visible = false;
				//_info.Value = null;
				_info.BindDocField.Value = null;
				_current_code = null;
			}
			else
			{
				_page.Display(_prefix + "InfoPanel");
				_page.Display(_prefix + "CodePanel");

				_info.Visible = true;
				_code.Visible = true;

				Person p = new Person(_person.Value);
				if (p.Unavailable)
				{
					_current_code = string.Empty;
				}
				else
				{
					_current_code = p.OKPO;

					if (string.IsNullOrEmpty(_store.Value))
					{
						_page.Store_BeforeSearch(_store, _person.Value);
						if(_store.TryFindSingleValue())
							_store.BindDocField.Value = _store.Value;
					}

					if (string.IsNullOrEmpty(_address.Value))
					{
						_page.Address_BeforeSearch(_address, _person.Value);
						if (_address.TryFindSingleValue())
						{
							if (null !=_address.BindDocField)
								_address.BindDocField.Value = _address.Value;
						}
					}
				}

				V4Control[] ctrls = { _person, _transport_node, _address };
				//_info.Value = GetHtmlPersonDescription(ctrls, ", ");
				_info.BindDocField.Value = GetHtmlPersonDescription(ctrls, ", ");
			}

			//_code.Value = _current_code;
			_code.BindDocField.Value = _current_code;

			_info.RenderNtf();
			_code.RenderNtf();

			UpdateBindedPanel(sender, args);
		}

		/// <summary>
		/// Обработчик события изменения расчетного счета лица
		/// </summary>
		/// <param name="sender">Объект источник события</param>
		/// <param name="args">Параметры события</param>
		private void Store_Changed(object sender, ProperyChangedEventArgs args)
		{
			Kesco.Lib.Entities.Item s = new Item();

		    if (null != _store.ValueObject)
		    {
		        s = (Kesco.Lib.Entities.Item) _store.ValueObject;
		    }
		    //ValueObject не коррелирует с Value, оно устанавливается только при выборе значения из списка
			if (string.IsNullOrEmpty(_store.Value) || null == s.Value)
			{
				_page.Hide(_prefix + "StoreInfoPanel");
				_store_info.Visible = false;
				//_store_info.Value = string.Empty;
				_store_info.BindDocField.Value = string.Empty;
				_current_store = null;
			}
			else
			{
				_current_store = new Kesco.Lib.Entities.Stores.Store(_store.Value);

				_page.Display(_prefix + "StoreInfoPanel");
				_store_info.Visible = true;
				//_store_info.Value = s.Value.ToString();
				_store_info.BindDocField.Value = s.Value.ToString();

				if (string.IsNullOrEmpty(_person.Value))
				{
					//_person.ValueInt = _current_store.ManagerCode;
					_person.BindDocField.Value = _current_store.ManagerCode.ToString();
					Person_Changed(null, null);
				}

				_store_info.RenderNtf();
			}

			UpdateBindedPanel(sender, args);
		}

		/// <summary>
		/// Обработчик события запроса уведомительного сообщения для описания расчетного счета лица
		/// </summary>
		/// <param name="sender">Объект источник события</param>
		/// <param name="ntf">Элемент отображающий уведомительно сообщение</param>
		void StoreInfo_OnRenderNtf(object sender, Ntf ntf)
		{
			ntf.Clear();

			if (string.IsNullOrEmpty(_store.Value)) return;

			Kesco.Lib.Entities.Item s;
            if (null != _store.ValueObject)
            {
                s = (Kesco.Lib.Entities.Item) _store.ValueObject;

                if (_store_info.Value != ((string) s.Value).TrimStart()) //В названии склада могут быть ведущие пробелы
                {
                    ntf.Add(n.ControlNtfs.InfoStore, NtfStatus.Error);
                }
            }
		}

		/// <summary>
		/// Обработчик события запроса уведомительного сообщения для описания лица
		/// </summary>
		/// <param name="sender">Объект источник события</param>
		/// <param name="ntf">Элемент отображающий уведомительно сообщение</param>
		void Info_OnRenderNtf(object sender, Ntf ntf)
		{
			ntf.Clear();

			if (string.IsNullOrEmpty(_info.Value)) return;

			V4Control[] GoInfoCtrls = new V4Control[] { _person, _transport_node, _address };

			string descr = GetHtmlPersonDescription(GoInfoCtrls.AsEnumerable(), ", ");

			bool display_ntf = false;

			if (null == _transport_node)
			{
				//Для поставщика и плательщика поле адрес сохраняется в договоре
				display_ntf = _info.Value != descr;
			}
			else
			{
				//Для грузоотправителя и грузополучателя поле адрес не сохраняется в договоре
				display_ntf = 0 != string.Compare(_info.Value, 0, descr, 0, descr.Length);
			}

			if (display_ntf)
			{
                ntf.Add(n.ControlNtfs.PersonInfo[_prefix], NtfStatus.Error);
			}
		}

		/// <summary>
		/// Обработчик события запроса уведомительного сообщения для ОКПО лица
		/// </summary>
		/// <param name="sender">Объект источник события</param>
		/// <param name="ntf">Элемент отображающий уведомительное сообщение</param>
		void CodeInfo_OnRenderNtf(object sender, Ntf ntf)
		{
			ntf.Clear();

			if (string.IsNullOrEmpty(_person.Value)) return;

            if (string.IsNullOrEmpty(_code.Value))
			{
                ntf.Add(n.ControlNtfs.PersonNoCode[_prefix], NtfStatus.Error);
				return;
			}

			if (_code.Value != _current_code)
			{
                ntf.Add(n.ControlNtfs.PersonCode[_prefix], NtfStatus.Error);
			}
		}
	}
}