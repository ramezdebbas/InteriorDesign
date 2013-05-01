using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace EcommFashion.Data
{
    /// <summary>
    /// Base class for <see cref="SampleDataItem"/> and <see cref="SampleDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : EcommFashion.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get { return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");

            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public SampleDataSource()
        {
            String ITEM_CONTENT = String.Format("Item Content: {0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}",
                        "Nivax Data");

            var group1 = new SampleDataGroup("Group-1",
                    "Directives",
                    "Group Subtitle: 1",
                    "Assets/DarkGray.png",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group1.Items.Add(new SampleDataItem("Group-1-Item-1",
                    "Interior Desinging",
                    "",
                    "Assets/HubPage/HubPage1.png",
                    "Interior design describes a group of various yet related projects that involve turning an interior space into an effective setting for the range of human activities that are to take place there.[1] An interior designer is someone who conducts such projects. Interior design is a multifaceted profession that includes conceptual development, liaising with the stakeholders of a project and the management and execution of the design.\n\nInterior design as carried out in the US is an almost entirely different practice to that carried out in the UK. This article describes interior design that relates mainly to the US.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-2",
                   "History",
                    "",
                    "Assets/HubPage/HubPage2.png",
                    "In the past, Interiors were put together instinctively as a part of the process of building.[1] The profession of interior design has been a consequence of the development of society and the complex architecture that has resulted from the development of industrial processes. The pursuit of effective use of space, user well-being and functional design has contributed to the development of the contemporary interior design profession. \n\nIn ancient India, architects used to work as interior designers. This can be seen from the references of Vishwakarma the architect - one of the Gods in Indian mythology. Additionally, the sculptures depecting ancient texts, events are seen in palaces built in 17th century India.\n\nThe Dark Ages led to a time of wood paneling, minimal furniture, and stone-slab floors. during the time people added a deccorative elements by putting wall fabrics and stone carvings. Coming out of the Dark Ages the work of color and ornamentation was introduced. And in the 12th century the Gothic Style came out and is noted for opened interiors and natural light.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-3",
                    "Designers",
                    "",
                    "Assets/HubPage/HubPage3.png",
                    "Interior Designer implies that there is more of an emphasis on Planning, Functional design and effective use of space involved in this profession, as compared to interior decorating. An interior designer can undertake projects that include arranging the basic layout of spaces within a building as well as projects that require an understanding of technical issues such as acoustics, lighting, temperature, etc.[1] Although an interior designer may create the layout of a space, they may not alter load-bearing walls without having their designs stamped for approval by an architect. Interior Designers often work directly with architectural firms.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-4",
                    "Residential",
                    "",
                    "Assets/HubPage/HubPage4.png",
                    "Residential design is the design of the interior of private residences. As this type design is very specific for individual situations, the needs and wants of the individual are paramount in this area of interior design. The interior designer may work on the project from the initial planning stage or may work on the remodelling of an existing structure. It is often a very involved process that takes months to fine tune and create a space with the vision of the client.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-5",
                    "Speciality",
                    "",
                    "Assets/HubPage/HubPage5.png",
                    "An interior designer may wish to specialize in a particular type of interior design in order to develop technical knowledge specific to that area. Types of interior design include residential design, commercial design, hospitality design, healthcare design, universal design, exhibition design, spatial branding, etc. The profession of Interior Design is relatively new, constantly evolving, and often confusing to the public. It is an art form that is consistently changing and evolving. Not only is it an art, but it also relies on research from many fields to provide a well-trained designer's understanding of how people are influenced by their environments. NCIDQ, the board for Interior Design qualifications, defines the profession in the best way: The Professional Interior Designer is qualified by education, experience, examination to enhance the function and quality of interior spaces.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-6",
                    "Conditional Settings",
                    "",
                    "Assets/HubPage/HubPage6.png",
                    "There are a wide range of working conditions and employment opportunities within interior design. Large and tiny corporations often hire interior designers as employees on regular working hours. Designers for smaller firms usually work on a contract or per-job basis. Self-employed designers, which make up 26% of interior designers,usually work the most hours. Interior designers often work under stress to meet deadlines, stay on budget, and meet clients' needs. In some cases, licensed professionals review the work and sign it before submitting the design for approval by clients or construction permisioning. The need for licensed review and signature varies by locality, relevant legislation, and scope of work. Their work can involve significant travel to visit different locations, however with technology development, the process of contacting clients and communicating design alternatives has become easier and requires less travel. They also renovate a space to satisfy the specific taste for a client.",
                    ITEM_CONTENT,
                    group1));
            this.AllGroups.Add(group1);

             var group2 = new SampleDataGroup("Group-2",
                    "Executions",
                    "Group Subtitle: 2",
                    "Assets/LightGray.png",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group2.Items.Add(new SampleDataItem("Group-2-Item-1",
					"Art Deco Style",
					"",
                    "Assets/HubPage/HubPage7.png",
                    "The Art Deco style began in Europe in the early years of the 20th century, with the waning of Art Nouveau. The term Art Deco was taken from the Exposition Internationale des Arts Decoratifs et Industriels Modernes, a world’s fair held in Paris in 1925.[10] Art Deco rejected many traditional classical influences in favor of more streamlined geometric forms and metallic color. The Art Deco style influenced all areas of design, especially interior design, because it was the first style of interior decoration to spotlight new technologies and materials.\n\nArt Deco style is mainly based on geometric shapes, streamlining and clean lines. The well-maintained Muswell Hill Odeon was an Art Deco style interior. Its striking lighting fixtures include an illuminated ribbon running down the middle of the ceiling to the top of the screen, which creates a streamlined effect, with a circular light be placed in the recessed ceiling area as a focal point. The geometrical shapes, angular edges and clean lines offer a sharp, cool look of mechanized living utterly at odds with anything that came before. The spacious lounge of Chicago’s 1929 Powhatan apartments which designed by Robert S. Degolyer and Charles L. Morgan is also a key Art Deco icon. These apartments note the geometric patterns on the ceiling’s light panels, as well as on the mouldings, grilles and pelmet. All of these geometric patterns provide by sharp angles and well-define lines that give the whole space a clean and elegant looking.",
                    ITEM_CONTENT,
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-2",
                    "Traditional Ways",
                    "",
                    "Assets/HubPage/HubPage8.png",
                    "As the influence of industrial power, the Art Deco has to be seemed as one of the most exciting decorative style of the century. The Art Deco reject the traditional materials of decoration and interior design, instead option to use more unusual materials such as chrome, glass, stainless steel, shiny fabrics, mirrors, aluminium, lacquer, inlaid wood, sharkskin, and zebra skin.\n\n Stemming from this use of harder, metallic materials is the celebration of the machine age. Some of the materials used in art deco style interiors are direct reflection of the time period. Materials like stainless steel, aluminium, lacquer, and inlaid woods all reflect the modern age that was ushered in after the end of the World War,and the steel and aluminium also reflect the growing aviation movement of the time. The innovative combinations of these materials create theatrical contrasts which were very popular at the end of the 1920s and during the 1930s, for example, the mixing highly polished wood and black lacquer with satin and furs.[16] The barber shop in the Austin Reed store in London was designed by P. J. Westwood. It was the trendiest barber shop in Britain by using metallic materials. \n\nThe whole barber shop was a gleaming ovoid space of mirrors, marble, chrome and frosted glass. The most exciting design was the undulating waves lighting fixture that forming by the continuous arcs of neon tubing, and support by chrome structure. The used of new technologies and materials emphasis the feature of Art Deco style.",
                    ITEM_CONTENT,
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-3",
                     "Japanese Materials",
                    "",
                    "Assets/HubPage/HubPage9.png",
                    "Japanese design is based strongly on craftsmanship, beauty, elaboration, and delicacy. The design of interiors is very simple but made with attention to detail and intricacy. This sense of intricacy and simplicity in Japanese designs is still valued in modern Japan as it was in traditional Japan.\n\nJapanese interior design is very efficient in the use of resources. Traditional and modern Japanese interiors have been flexible in use and designed mostly with natural materials. The spaces are used as multifunctional rooms. The rooms can be opened to create more space for an occasion or more private and closed-off by pulling closed paper screens called shoji. A large portion of Japanese interior walls are often made of shoji screens that can be pushed opened to join two rooms together, and then close them allowing more privacy. The shoji screens are made of paper attached in thin wooden frames that roll away on a track when they are pushed opened. Another large importance of the shoji screen besides privacy and seclusion is that they allow light through. This is an important aspect to Japanese design. Paper translucent walls allow light to be diffused through the space and create light shadows and patterns. Another way to connect rooms in Japan’s interiors is through Sliding panels made of wood and paper, like the shoji screens, or cloth. These panels are called Fusuma and are used as an entire wall. They are traditionally hand painted.",
                    ITEM_CONTENT,
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-4",
                    "Texture",
                   "",
                   "Assets/HubPage/HubPage10.png",
                   "Since the furniture and lighting fixture are the very significant parts of interior design, the features of Art Deco style also work the same in furniture and lighting design as well. Art Deco Furnishings and lighting fixtures have a glossy, luxurious appearance. Art Deco is a streamlined, geometric style which often includes furniture pieces with curved edges, geometric shapes and clean lines. Art deco furniture use glossy and shiny with inlaid wood and reflective finishes. The materials of chrome, aluminium, glass, mirrors and lacquered wood can create glossy and brilliant surfaces that define this style. Art Deco lighting fixtures often make use of the stacked geometric patterns. Most fixtures were made from polished bronze, chrome or steel in order to create that shiny, sleek look that was most associated with Art Deco.",
                   ITEM_CONTENT,
                   group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-5",
                    "Internal Ways",
                   "",
                   "Assets/HubPage/HubPage11.png",
                   "Interior design was previously seen as playing a secondary role to architecture. It also has many connections to other design disciplines, involving the work of architects, industrial designers, engineers, builders, craftsmen, etc. For these reasons the government of interior design standards and qualifications was often incorporated into other professional organisations that involved design.\n\nOrganisations such as the Chartered Society of Designers, established in the UK in 1986, and the American Designers Institute, founded in 1938, were established as organisations that governed various areas of design. It was not until later that specific representation for the interior design profession was developed. The US National Society of Interior Designers was established in 1957, while in the UK the Interior Decorators and Designers Association was established in 1966. Across Europe, other organisations such as The Finnish Association of Interior Architects (1949) were being established and in 1994 the International Interior Design Association was founded.",
                   ITEM_CONTENT,
                   group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-6",
                    "Professional Ways",
                   "",
                   "Assets/HubPage/HubPage12.png",
                   "Other areas of specialisation include museum and exhibition design, event design (including ceremonies, parties, conventions and concerts), theatre and performance design, production design for film and television. Beyond those, interior designers, particularly those with graduate education, can specialize in healthcare design, gerontological design, educational facility design, and other areas that require specialized knowledge. Some university programs offer graduate studies in theses and other areas. For example, both Cornell University and University of Florida offer interior design graduate programs in environment and behavior studies. Within this at University of Florida, students may choose a specific focus such as retirement community design (under Dr. Nichole Campbell) co-housing (Dr. Maruja Torres) or theft prevention by design (Prof. Candy Carmel-Gilfilen) (Campbell, 2012, Personal Communication).",
                   ITEM_CONTENT,
                   group2));
            this.AllGroups.Add(group2); 


        }
    }
}
