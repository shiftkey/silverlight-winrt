using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ImageSearch
{
#if SILVERLIGHT
  public class ObservableVector<T> : ObservableCollection<T> {}
#endif
}
