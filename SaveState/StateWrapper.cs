/*
 * Copyright (c) 2024 Rune Skovbo Johansen
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace Runevision.SaveState {

	public abstract class StateWrapper {
		public delegate void ValueChanged();
		public ValueChanged valueChanged;
		public abstract void SetDefault();
		public abstract object objectValue { get; set; }
	}

	public class StateWrapper<T> : StateWrapper {
		T value;
		T defaultValue;

		public T Value {
			get { return value; }
			set {
				if (this.value.Equals(value))
					return;
				this.value = value;
				if (valueChanged != null)
					valueChanged.Invoke();
			}
		}

		public StateWrapper() { }
		public StateWrapper(T value) { this.Value = value; defaultValue = value; }
		public override string ToString() { return Value.ToString(); }
		public override void SetDefault() { Value = defaultValue; }
		public override object objectValue { get { return (object)Value; } set { this.Value = (T)value; } }
	}

}
