using spine_2._1._25_netcore;

namespace MapleLib.WzLib.Spine {
	/// <summary>
	/// 
	/// </summary>
	public class WzSpineObject {
		public readonly WzSpineAnimationItem spineAnimationItem;

		public Skeleton skeleton;
		public AnimationStateData stateData;
		public AnimationState state;
		public SkeletonBounds bounds = new SkeletonBounds();


		public WzSpineObject(WzSpineAnimationItem spineAnimationItem) {
			this.spineAnimationItem = spineAnimationItem;
		}
	}
}