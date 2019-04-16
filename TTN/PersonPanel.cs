using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.DBSelect.V4;
using Kesco.Lib.Entities.Stores;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.Entities.Documents;
using Kesco.Lib.Entities.Persons.PersonOld;
using Kesco.Lib.Entities.Resources;
using Item = Kesco.Lib.Entities.Item;

namespace Kesco.App.Web.Docs.TTN
{
	//Класс объекта-контроллера определяющего поведение панели объединяющей взаимосвязанные элементы пользователького интерфейса
	//Панель используется для отображения и редактирования свойств лиц: поставщик, плательщик, грузоотправитель, грузополучатель
	public class PersonPanel
    {
        #region Declaration
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
        private TextBox _address;
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
        #endregion

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
        public static PersonPanel Init(Nakladnaya page, string prefix, TextBox info, TextBox code, DBSPerson person, TextBox address, DBSTransportNode transport_node, DBSStore store, TextBox store_info, TextBox notes)
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
        private PersonPanel(Nakladnaya page, string prefix, TextBox info, TextBox code, DBSPerson person, TextBox address, DBSTransportNode transport_node, DBSStore store, TextBox store_info, TextBox notes)
        {
            n = page;

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
            _address.Changed += Address_Changed;
            _store.Changed += Store_Changed;

            if (null != _transport_node)
                _transport_node.Changed += Transport_Node_Changed;

            if (null != _notes)
                _notes.Changed += Notes_Changed;
            
            _person.BeforeSearch += s => _page.Person_BeforeSearch(_person);
            _store.BeforeSearch += s => _page.Store_BeforeSearch(_store, _person.Value);
            //_address.BeforeSearch += s => _page.Address_BeforeSearch(_address, _person.Value);
            _person.OnRenderNtf += (s, ntf) => _page.Person_OnRenderNtf(ntf, _person, _prefix);
			//_address.OnRenderNtf += (s, ntf) => _page.Address_OnRenderNtf(ntf, _person, _address, _prefix);
			_store.OnRenderNtf += (s, ntf) => _page.Store_OnRenderNtf(ntf, _person, _current_store, _prefix);

			_store.GetFilter().StoreTypeId.Value = "1,2,3";

			_info.OnRenderNtf += Info_OnRenderNtf;
			_store_info.OnRenderNtf += StoreInfo_OnRenderNtf;

            // Для обновления видимости контролов
            if (!_page.Doc.IsNew || _page.Request["CopyDoc"] != null)
            {
                UpdateFieldVisibility();
            }

            _info.RenderNtf();
            _person.RenderNtf();
            _store.RenderNtf();
			_store_info.RenderNtf();
		}

        /// <summary>
        /// Установка адреса
        /// </summary>
        /// <param name="address"></param>
        public void SetAddress(string address)
        {

            if (_address.BindDocField != null)
            {
                _address.BindDocField.Value = address;
            }
            _address.Value = address;

            BindFieldsByPerson();

            ClientScripts.SendSetInnerHtml(_page, _prefix + Nakladnaya.suffixTitle, GetTitle());
        }

        /// <summary>
        /// Установка лица
        /// </summary>
        /// <param name="id"></param>
        /// <param name="address"></param>
        /// <param name="info"></param>
        public void SetPerson(int id, string address, string info)
	    {
            if (_notes != null && _notes.BindDocField != null) _notes.BindDocField.Value = "";
            _code.BindDocField.Value = "";
            _info.BindDocField.Value = "";
            _store.BindDocField.Value = "";

            _person.BindDocField.Value = id.ToString();
            if (_address.BindDocField != null) _address.BindDocField.Value = address;
            _address.Value = address;
            _info.BindDocField.Value = info;

            BindFieldsByPerson();

            ClientScripts.SendSetInnerHtml(_page, _prefix + Nakladnaya.suffixTitle, GetTitle());
	    }

	    /// <summary>
		/// Метод для установки лица
		/// </summary>
		/// <param name="id">Код лица</param>
		public void SetPerson(int id)
		{
			V4Control[] ctrls = { _info, _code, _person, _address,  _transport_node, _store, _store_info, _notes };

			foreach (V4Control ctrl in ctrls)
			{
				if (null == ctrl) continue;
                if (ctrl.BindDocField != null)
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

            BindFieldsByPerson();

			ClientScripts.SendSetInnerHtml(_page, _prefix + Nakladnaya.suffixTitle, GetTitle());
            
		}

		/// <summary>
		/// Метод связывает значения полей в двух панелях (Грузоотправитель-Поставщик или Грузополучатель-Плательщик)
		/// </summary>
		/// <param name="p">Ссылка на связываемую панель</param>
		public void BindTo(PersonPanel p)
		{
			//PersonPanel prev = _p ?? p;
			_p = p;
            /*
			if (null != prev)
			{
				prev._person.IsReadOnly = null != p;
                prev._address.IsReadOnly = null != p;
				prev._store.IsReadOnly = null != p;
			}
            */
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
            if (_p._address.BindDocField != null) _p._address.BindDocField.Value = _address.Value;
			_p._store.BindDocField.Value = _store.Value;

            _p.BindFieldsByPerson();
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

			foreach (var ctrl in ctrls)
			{
				if (null == ctrl) continue;
                
				string textOfControl = Nakladnaya.GetCtrlText(ctrl, false, n.CurrentUser);

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
            var disabledcode = _store.IsDisabled;
            _store.IsDisabled = true;
            V4Control[] ctrls = { _person, _transport_node, _notes, _address, _info, _code, _store, _store_info };
            string dataInfo = n.GetSectionHtmlDescription(ctrls);
            _store.IsDisabled = disabledcode;
            //if (_prefix != "Shipper")
            //    _page.JS.Write("SetExpandAccordionByControl('{0}_1');", _store.ClientID);

			string strNtfInfo = string.Empty;

			using (StringWriter ntfText = new StringWriter())
			{
				//_info.RenderNtf();
				_info.RenderNtf(ntfText);

				//_code.RenderNtf();
				//_code.RenderNtf(ntfText);

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
            _info.RenderNtf();
            _code.RenderNtf();
		}

        #region Changed
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
            Store_Changed(null, null);
            _store.RenderNtf();
            ClientScripts.SendSetInnerHtml(_page, _prefix + Nakladnaya.suffixTitle, GetTitle());
        }

        /// <summary>
        /// Установки видимости контролов
        /// </summary>
        public void UpdateFieldVisibility()
	    {
            if (string.IsNullOrEmpty(_person.Value))
            {
                _page.Hide(_prefix + "InfoPanel");
                _page.Hide(_prefix + "CodePanel");

                _info.Visible = false;
                _code.Visible = false;
                _address.Value = null;
                if (_info.BindDocField != null)
                    _info.BindDocField.Value = null;
                _current_code = null;
            }
            else
            {
                _page.Display(_prefix + "InfoPanel");
                _page.Display(_prefix + "CodePanel");

                _info.Visible = true;
                _code.Visible = true;
            }

            if (string.IsNullOrEmpty(_store.Value) || null == _store.ValueObject)
            {
                _page.Hide(_prefix + "StoreInfoPanel");
                _store_info.Visible = false;
                if (_store_info.BindDocField != null)
                    _store_info.BindDocField.Value = string.Empty;
                _current_store = null;
            }
            else
            {
                _current_store = _page.GetObjectById(typeof (Store), _store.Value) as Store;
                _page.Display(_prefix + "StoreInfoPanel");
                _store_info.Visible = true;
            }

	    }

	    /// <summary>
        /// Обработчик события изменения лица
        /// </summary>
        /// <param name="sender">Объект источник события</param>
        /// <param name="args">Параметры события</param>
        public void Person_Changed(object sender, ProperyChangedEventArgs args)
	    {
            if (_address.BindDocField != null) _address.BindDocField.Value = "";
	        _address.Value = "";
            if (_notes != null && _notes.BindDocField != null) _notes.BindDocField.Value = "";
            _code.BindDocField.Value = "";
            _info.BindDocField.Value = "";
            _store.BindDocField.Value = "";

            BindFieldsByPerson();
        }

        public void Address_Changed(object sender, ProperyChangedEventArgs args)
        {
            BindFieldsByPerson();
        }

        public void Notes_Changed(object sender, ProperyChangedEventArgs args)
        {
            BindFieldsByPerson();
        }

	    public void BindFieldsByPerson()
	    {
            UpdateFieldVisibility();
            if (!string.IsNullOrEmpty(_person.Value))
            {
                PersonOld p = _page.GetObjectById(typeof(PersonOld), _person.Value) as PersonOld;
                if (p.Unavailable)
                {
                    _current_code = string.Empty;
                }
                else
                {
                    _current_code = p.OKPO;
                    /*
                    if (string.IsNullOrEmpty(_store.Value))
                    {
                        _page.Store_BeforeSearch(_store, _person.Value);
                        //if (_store.TryFindSingleValue()) _store.BindDocField.Value = _store.Value;
                    }
                    */
                    //var card = p.GetCard(_page.Doc.Date == DateTime.MinValue ? DateTime.Today : _page.Doc.Date);
                    var card = _page.GetCardById(p, _page.Doc.Date == DateTime.MinValue ? DateTime.Today : _page.Doc.Date);

                    if (_prefix == "Shipper" || _prefix == "Payer")
                    {
                        if (string.IsNullOrEmpty(_address.Value))
                        {
                            if (card != null)
                            {
                                _address.Value = card.АдресЮридический.Length == 0
                                    ? card.АдресЮридическийЛат
                                    : card.АдресЮридический;
                            }

                            if (null != _address.BindDocField)
                                _address.BindDocField.Value = _address.Value;
                        }
                    }

                    // Название поставщика/плательщика реквизиты ГО/ГП
                    if (card != null)
                    {
                        _info.BindDocField.Value = card.NameRus.Length > 0 ? card.NameRus : card.NameLat;
                    }

                    if (_prefix == "Go" || _prefix == "Gp")
                    {
                        V4Control[] ctrls = { _transport_node, _address, _notes };
                        var addInfo = GetHtmlPersonDescription(ctrls, ", ");
                        _info.BindDocField.Value += (_info.Value.Length > 0 && addInfo.Length > 0 ? ", " : "") + addInfo;
                    }
                }
            }

            _code.BindDocField.Value = _current_code;
            UpdatePersonDeps();
	    }

	    /// <summary>
	    /// Обработчик события изменения пункта отправления
	    /// </summary>
	    /// <param name="sender">Объект источник события</param>
	    /// <param name="args">Параметры события</param>
        private void Transport_Node_Changed(object sender, ProperyChangedEventArgs args)
	    {
            if (!_transport_node.Value.IsNullEmptyOrZero())
                _notes.Value = Document.GetTRWeselLastNote(_transport_node.Value, _page.Doc.TypeID.ToString(), _page.Doc.DocId, (_page.Doc.Date != DateTime.MinValue ? _page.Doc.Date : DateTime.Today));
            BindFieldsByPerson();
	    }

	    /// <summary>
        /// Обработчик события изменения расчетного счета лица
        /// </summary>
        /// <param name="sender">Объект источник события</param>
        /// <param name="args">Параметры события</param>
        private void Store_Changed(object sender, ProperyChangedEventArgs args)
        {
            Item s = new Item();

            if (null != _store.ValueObject)
            {
                s = (Item)_store.ValueObject;
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
                //_current_store = new Store(_store.Value);
                _current_store = _page.GetObjectById(typeof(Store), _store.Value) as Store;

                _page.Display(_prefix + "StoreInfoPanel");
                _store_info.Visible = true;
                //_store_info.Value = s.Value.ToString();

                PersonOld p = _page.GetObjectById(typeof(PersonOld), _current_store.KeeperId.ToString()) as PersonOld;

                if (p != null && p.Id != "")
                {
                    //var card = p.GetCard(_page.Doc.Date == DateTime.MinValue ? DateTime.Today : _page.Doc.Date);
                    var card = _page.GetCardById(p, _page.Doc.Date == DateTime.MinValue ? DateTime.Today : _page.Doc.Date);
                    if (card != null && card.NameRus.Length == 0 && card.NameLat.Length == 0) return;
                    var crdName = "";
                    if (card != null)
                    {
                        crdName = " в " + (card.NameRus.Length > 0 ? card.NameRus : card.NameLat);
                    }

                    _store_info.BindDocField.Value = "№" + s.Value + crdName;

                    if (_current_store.Subsidiary.Length > 0) _store_info.BindDocField.Value += " в " + _current_store.Subsidiary;

                    if (p.BIK.Length > 0 && ((Kesco.Lib.Entities.Documents.EF.Trade.TTN)_page.Doc).CurrencyField.ValueString == ((int)Currency.Code.RUR).ToString()) _store_info.BindDocField.Value += ", БИК " + p.BIK;
                    if (p.SWIFT.Length > 0 && ((Kesco.Lib.Entities.Documents.EF.Trade.TTN)_page.Doc).CurrencyField.ValueString.Length > 0 && !((Kesco.Lib.Entities.Documents.EF.Trade.TTN)_page.Doc).CurrencyField.ValueString.Equals(((int)Currency.Code.RUR).ToString())) _store_info.BindDocField.Value += ", SWIFT " + p.SWIFT;

                    if (p.CorrAccount.Length > 0) _store_info.BindDocField.Value += ", КС " + p.CorrAccount;

                }

                if (string.IsNullOrEmpty(_person.Value))
                {
                    //_person.ValueInt = _current_store.ManagerId;
                    _person.BindDocField.Value = _current_store.ManagerId.ToString();
                    BindFieldsByPerson();
                }

                _store_info.RenderNtf();
            }

            UpdateBindedPanel(sender, args);
        }
        #endregion

        #region OnRenderNtf
        /// <summary>
        /// Обработчик события запроса уведомительного сообщения для описания расчетного счета лица
        /// </summary>
        /// <param name="sender">Объект источник события</param>
        /// <param name="ntf">Элемент отображающий уведомительно сообщение</param>
        void StoreInfo_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();

            if (string.IsNullOrEmpty(_store.Value)) return;
            //todo: РАЗОБРАТЬСЯ
            //Kesco.Lib.Entities.Item s;
            //if (null != _store.ValueObject)
            //{
            //    s = (Kesco.Lib.Entities.Item)_store.ValueObject;

            //    if (_store_info.Value != ((string)s.Value).TrimStart()) //В названии склада могут быть ведущие пробелы
            //    {
            //        ntf.Add(n.ControlNtfs.InfoStore, NtfStatus.Error);
            //    }
            //}
        }

        /// <summary>
        /// Обработчик события запроса уведомительного сообщения для описания лица
        /// </summary>
        /// <param name="sender">Объект источник события</param>
        /// <param name="ntf">Элемент отображающий уведомительно сообщение</param>
        void Info_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();

            /*
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
            */
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
        #endregion

	}
}